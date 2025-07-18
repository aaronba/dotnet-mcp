using McpServer.Models;
using McpServer.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ImageMagick;

namespace McpServer.Tools;

/// <summary>
/// PDF tool for converting PDF files to PNG images as base64
/// </summary>
public class PdfTool : IToolHandler
{
    private readonly ILogger<PdfTool> _logger;

    public PdfTool(ILogger<PdfTool> logger)
    {
        _logger = logger;
    }

    public static McpTool GetToolDefinition()
    {
        return new McpTool
        {
            Name = "pdf_to_png",
            Description = "Convert PDF file pages to PNG images as base64 strings for use with Azure OpenAI GPT 4.1 vision/OCR input. Use page_number='*' to convert all pages.",
            InputSchema = new ToolInputSchema
            {
                Type = "object",
                Properties = new Dictionary<string, ToolProperty>
                {
                    ["file_path"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Path to the PDF file to convert (optional if pdf_data is provided)"
                    },
                    ["pdf_data"] = new ToolProperty
                    {
                        Type = "string", 
                        Description = "Base64 encoded PDF data (optional if file_path is provided)"
                    },
                    ["page_number"] = new ToolProperty
                    {
                        Type = "string",
                        Description = "Page number to convert (1-based, defaults to 1) or '*' to convert all pages"
                    },
                    ["quality"] = new ToolProperty
                    {
                        Type = "number", 
                        Description = "Image quality/DPI (defaults to 150)"
                    }
                },
                Required = new List<string>()
            }
        };
    }

    public async Task<ToolCallResult> HandleAsync(ToolCallParams toolCall, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = GetStringValue(toolCall.Arguments.GetValueOrDefault("file_path"));
            var pdfData = GetStringValue(toolCall.Arguments.GetValueOrDefault("pdf_data"));
            var pageNumberParam = GetStringValue(toolCall.Arguments.GetValueOrDefault("page_number", "1"));
            var quality = GetIntValue(toolCall.Arguments.GetValueOrDefault("quality", 150));

            if (string.IsNullOrEmpty(filePath) && string.IsNullOrEmpty(pdfData))
            {
                return new ToolCallResult
                {
                    IsError = true,
                    Content = new List<ToolContent>
                    {
                        new ToolContent
                        {
                            Type = "text",
                            Text = "Either file_path or pdf_data must be provided"
                        }
                    }
                };
            }

            byte[] pdfBytes;
            
            // Get PDF data either from file or base64 string
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!File.Exists(filePath))
                {
                    return new ToolCallResult
                    {
                        IsError = true,
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = $"File not found: {filePath}"
                            }
                        }
                    };
                }

                pdfBytes = await File.ReadAllBytesAsync(filePath, cancellationToken);
                _logger.LogInformation("Read PDF file: {FilePath} ({Size} bytes)", filePath, pdfBytes.Length);
            }
            else
            {
                try
                {
                    pdfBytes = Convert.FromBase64String(pdfData!);
                    _logger.LogInformation("Decoded PDF from base64 ({Size} bytes)", pdfBytes.Length);
                }
                catch (FormatException ex)
                {
                    return new ToolCallResult
                    {
                        IsError = true,
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = $"Invalid base64 PDF data: {ex.Message}"
                            }
                        }
                    };
                }
            }

            // Check if we should convert all pages or a specific page
            bool convertAllPages = pageNumberParam == "*";
            var content = new List<ToolContent>();

            if (convertAllPages)
            {
                // Convert all pages
                var base64Images = new List<string>();
                
                using (var magickImages = new MagickImageCollection())
                {
                    var readSettings = new MagickReadSettings
                    {
                        Density = new Density(quality, quality),
                        Format = MagickFormat.Pdf
                    };

                    magickImages.Read(pdfBytes, readSettings);
                    
                    for (int i = 0; i < magickImages.Count; i++)
                    {
                        var magickImage = magickImages[i];
                        
                        // Set output format to PNG
                        magickImage.Format = MagickFormat.Png;
                        
                        // Optimize for web/vision models
                        magickImage.Strip(); // Remove metadata
                        magickImage.Quality = 90; // High quality for OCR

                        // Convert to base64
                        var imageBytes = magickImage.ToByteArray();
                        var base64Image = Convert.ToBase64String(imageBytes);
                        base64Images.Add(base64Image);
                        
                        _logger.LogInformation("Converted PDF page {Page} to PNG ({Width}x{Height}, {Size} bytes)", 
                            i + 1, magickImage.Width, magickImage.Height, imageBytes.Length);
                    }
                }

                content.Add(new ToolContent
                {
                    Type = "text",
                    Text = $"Successfully converted all {base64Images.Count} pages of PDF to PNG images (base64 encoded, ready for Azure OpenAI GPT 4.1 vision input)"
                });

                // Add each page as a separate content item for easier processing
                for (int i = 0; i < base64Images.Count; i++)
                {
                    content.Add(new ToolContent
                    {
                        Type = "text",
                        Text = $"Page {i + 1}: {base64Images[i]}"
                    });
                }
            }
            else
            {
                // Convert specific page
                int pageNumber;
                if (!int.TryParse(pageNumberParam, out pageNumber) || pageNumber < 1)
                {
                    return new ToolCallResult
                    {
                        IsError = true,
                        Content = new List<ToolContent>
                        {
                            new ToolContent
                            {
                                Type = "text",
                                Text = "page_number must be a positive integer or '*' for all pages"
                            }
                        }
                    };
                }

                string base64Image;
                
                using (var magickImage = new MagickImage())
                {
                    var readSettings = new MagickReadSettings
                    {
                        Density = new Density(quality, quality),
                        Format = MagickFormat.Pdf
                    };

                    // Read the specific page (ImageMagick is 0-based, so subtract 1)
                    readSettings.FrameIndex = (uint)(pageNumber - 1);
                    readSettings.FrameCount = 1;

                    magickImage.Read(pdfBytes, readSettings);
                    
                    // Set output format to PNG
                    magickImage.Format = MagickFormat.Png;
                    
                    // Optimize for web/vision models
                    magickImage.Strip(); // Remove metadata
                    magickImage.Quality = 90; // High quality for OCR

                    // Convert to base64
                    var imageBytes = magickImage.ToByteArray();
                    base64Image = Convert.ToBase64String(imageBytes);
                    
                    _logger.LogInformation("Converted PDF page {Page} to PNG ({Width}x{Height}, {Size} bytes)", 
                        pageNumber, magickImage.Width, magickImage.Height, imageBytes.Length);
                }

                content.Add(new ToolContent
                {
                    Type = "text",
                    Text = $"Successfully converted PDF page {pageNumber} to PNG image (base64 encoded, ready for Azure OpenAI GPT 4.1 vision input)"
                });
                
                content.Add(new ToolContent
                {
                    Type = "text",
                    Text = base64Image
                });
            }

            return await Task.FromResult(new ToolCallResult
            {
                IsError = false,
                Content = content
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to PNG");
            return new ToolCallResult
            {
                IsError = true,
                Content = new List<ToolContent>
                {
                    new ToolContent
                    {
                        Type = "text",
                        Text = $"PDF conversion error: {ex.Message}"
                    }
                }
            };
        }
    }

    private static string? GetStringValue(object? value)
    {
        return value switch
        {
            JsonElement element when element.ValueKind == JsonValueKind.String => element.GetString(),
            string s => s,
            null => null,
            _ => value.ToString()
        };
    }

    private static int GetIntValue(object? value)
    {
        return value switch
        {
            JsonElement element when element.ValueKind == JsonValueKind.Number => element.GetInt32(),
            int i => i,
            long l => (int)l,
            double d => (int)d,
            string s when int.TryParse(s, out var parsed) => parsed,
            _ => Convert.ToInt32(value)
        };
    }
}