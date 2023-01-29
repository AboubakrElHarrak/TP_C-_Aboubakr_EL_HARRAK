using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ContactManager.Serialisation
{
    public class SerializerFactory
    {
        public ISerializer CreateSerializer(string type)
        {
            if (type.ToLower() == "xml")
            {
                return new XmlSerializer();
            }
            else if (type.ToLower() == "binaire")
            {
                return new BinarySerializer();
            }
            else
            {
                throw new ArgumentException("Type de serialisation invalid !");
            }

        }
    }
}
