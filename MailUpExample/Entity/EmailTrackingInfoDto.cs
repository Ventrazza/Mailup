using System.Collections.Generic;

namespace MailUpExample.Entity {
    public class EmailTrackingInfoDto {
        public bool Enabled { get; set; }
        public List<string> Protocols { get; set; }
        public string CustomParams { get; set; }

        public string ProtocolsToString() {
            string ret = "";
            for (int p = 0; p < Protocols.Count; p++) {
                string curProtocol = Protocols[p];

                ret += "" + curProtocol.Replace(":", "") + ":";
                if (p < (Protocols.Count - 1)) {
                    ret += "|";
                }
            }

            return ret;
        }
        public string CustomParamsToString() {
            string ret = "";
            if (!string.IsNullOrEmpty(CustomParams)) {
                if (CustomParams.StartsWith("?"))
                    ret = CustomParams.Substring(1);
                else
                    ret = CustomParams;
            }
            return ret;
        }
    }
}