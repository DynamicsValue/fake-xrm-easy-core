namespace FakeXrmEasy.Core.FileStorage
{
    /// <summary>
    /// Contains the current organization settings that are relevant to file storage
    /// </summary>
    internal class OrganizationFileSettings
    {
        internal string[] BlockedAttachments { get; set; }
        internal string[] AllowedMimeTypes { get; set; }
        internal string[] BlockedMimeTypes { get; set; }

        private const char separator = ';';
        
        private const string DEFAULT_BLOCKED_ATTACHMENTS =
            @"ade;adp;app;asa;ashx;asmx;asp;bas;bat;cdx;cer;chm;class;cmd;com;config;cpl;crt;csh;dll;exe;fxp;hlp;hta;htr;htw;ida;idc;idq;inf;ins;isp;its;jar;js;jse;ksh;lnk;mad;maf;mag;mam;maq;mar;mas;mat;mau;mav;maw;mda;mdb;mde;mdt;mdw;mdz;msc;msh;msh1;msh1xml;msh2;msh2xml;mshxml;msi;msp;mst;ops;pcd;pif;prf;prg;printer;pst;reg;rem;scf;scr;sct;shb;shs;shtm;shtml;soap;stm;tmp;url;vb;vbe;vbs;vsmacros;vss;vst;vsw;ws;wsc;wsf;wsh;svg";
        
        internal OrganizationFileSettings()
        {
            BlockedAttachments = FromCommaSeparated(DEFAULT_BLOCKED_ATTACHMENTS);
            AllowedMimeTypes = new string[] { };
            BlockedMimeTypes = new string[] { };
        }

        internal static string[] FromCommaSeparated(string commaSeparatedValues)
        {
            if (commaSeparatedValues.IndexOf(separator) >= 0)
            {
                return commaSeparatedValues.Split(separator);
            }

            if (!string.IsNullOrWhiteSpace(commaSeparatedValues))
            {
                return new string[] { commaSeparatedValues };
            }

            return new string[] { }; 
        }
    }
}