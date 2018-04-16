using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PackageManager.Serialization
{
    public static class SerializationExtensions
    {

         ///<summary>
         ///Provides a byte[] array of survey data for mass storage.
         ///</summary>
         ///<returns>a byte[] array of binary survey data.</returns>
        public static byte[] SerializeBinary(object source)
        {
            try
            {
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            formatter.Serialize(stream, source);
            Byte[] theArray = stream.ToArray();
            return theArray;
                        }
            catch (Exception x)
            {
                return Encoding.ASCII.GetBytes("Error: " + x.ToString());
            }
        }
        /// <summary>
        /// /Provides deserialization from a byte buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static T DeSerializeBinary<T>(byte[] buffer)
        {
            //try
            //{
                IFormatter formatter = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(buffer);
                return (T)formatter.Deserialize(stream);
            //}
            //catch //(Exception x)         
            //{
            //    return default(T);
            //}
        }

        public static XElement Serialize(this object source)
        {
            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");


            //try
            //{

                var serializer = XmlSerializerFactory.GetSerializerFor(source.GetType());
                var xdoc = new XDocument();
                using (var writer = xdoc.CreateWriter())
                {
                    serializer.Serialize(writer, source,ns);//, new XmlSerializerNamespaces(new[] { new XmlQualifiedName("", "") }));
                }
                return (xdoc.Document != null) ? xdoc.Document.Root : new XElement("Error", "Document Missing");
            //}
            //catch (Exception x)
            //{
            //    return new XElement("Error", x.ToString());
            //}
        }
        public static T Deserialize<T>(this XElement source) where T : class
        {
            try
            {
                var serializer = XmlSerializerFactory.GetSerializerFor(typeof(T));
                return (T)serializer.Deserialize(source.CreateReader());
            }
            catch (Exception x)         
            {
                MessageBox.Show(x.Message + " " + x.InnerException.Message);
                return null;
            }
        }
    }
    public static class XmlSerializerFactory
    {
        private static Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();
        public static XmlSerializer GetSerializerFor(Type typeOfT)
        {
            if (!serializers.ContainsKey(typeOfT))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("XmlSerializerFactory.GetSerializerFor(typeof({0}));", typeOfT));
                var newSerializer = new XmlSerializer(typeOfT);
                serializers.Add(typeOfT, newSerializer);
            }
            return serializers[typeOfT];
        }


                /// <summary>
        /// Privides a byte[] array of survey data for mass storage.
        /// </summary>
        /// <returns>a byte[] array of binary survey data.</returns>
        //public byte[] Serialize()
        //{
        //    IFormatter formatter = new BinaryFormatter();
        //    MemoryStream stream = new MemoryStream();
        //    formatter.Serialize(stream, this);
        //    Byte[] theArray = stream.ToArray();
        //    return theArray;
        //}

    }
    //#region Serialization
    //    /// <summary>
    //    /// /Provides deserialization from a byte buffer.
    //    /// </summary>
    //    /// <param name="buffer"></param>
    //    /// <returns></returns>
    //    public static [TYPE] DeSerialize(byte[] buffer)
    //    {
    //        System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    //        System.IO.MemoryStream stream = new System.IO.MemoryStream(buffer);
    //        return formatter.Deserialize(stream) as [TYPE]
    //    }

    //    /// <summary>
    //    /// Privides a byte[] array of survey data for mass storage.
    //    /// </summary>
    //    /// <returns>a byte[] array of binary survey data.</returns>
    //    public byte[] Serialize()
    //    {
    //        System.Runtime.Serialization.IFormatter formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
    //        System.IO.MemoryStream stream = new System.IO.MemoryStream();
    //        formatter.Serialize(stream, this);
    //        Byte[] theArray = stream.ToArray();
    //        return theArray;
    //    }
    //    #endregion
}
