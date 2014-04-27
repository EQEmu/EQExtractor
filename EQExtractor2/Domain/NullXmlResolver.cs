using System;
using System.IO;
using System.Net;
using System.Xml;

namespace EQExtractor2.Domain
{
    /// <summary>
    /// XMLResolver that actuyally does nothing; it just passes a memory stream back so that the reader doesn't complain.
    /// I hate complaints.
    /// </summary>
    public class NullXmlResolver : XmlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            return new MemoryStream();
        }

        public override ICredentials Credentials
        {
            set
            {
            }
        }
    }
}