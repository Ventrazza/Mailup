using System.Collections.Generic;

namespace MailUpExample.Entity {
    public class EmailMessageItemDto {
        public EmailTrackingInfoDto TrackingInfo = new EmailTrackingInfoDto();
        public List<EmailDynamicFieldDto> Fields { get; set; }
        public List<EmailTagDto> Tags { get; set; }
        public int IdList { get; set; }
        public int IdNL { get; set; }
        public bool Embed { get; set; }
        public bool IsConfirmation { get; set; }
        public string Subject { get; set; }
        public string Notes { get; set; }
        public string Content { get; set; }                    
        public string Head { get; set; }
        public string Body { get; set; }
        public EmailMessageItemDto() {
            Body = "<body>";
        }
    }
}