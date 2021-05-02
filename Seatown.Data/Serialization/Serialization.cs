using System;
using System.Data;

namespace Seatown.Data
{
    public static class Serialization
    {
        // TODO: Serialize objects based on the format specified
        // TODO: Add a default format property to the class, but allow methods to override this value.

        #region Properties

        public static SerializationFormats DefaultSerializationFormat { get; set; } = SerializationFormats.XML;

        #endregion

        #region DataSet Helper Methods


        public static DataSet DataSetFromXmlString(string serializedObject)
        {
            return DataSetFromXmlString(serializedObject, false);
        }

        public static DataSet DataSetFromXmlString(string serializedObject, bool removeFirstElement)
        {
            DataSet result = null;
            if (!string.IsNullOrEmpty(serializedObject))
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(serializedObject)))
                {
                    using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(ms))
                    {
                        result = DataSetFromXmlString(reader, removeFirstElement);
                    }
                }
            }
            return result;
        }

        public static DataSet DataSetFromXmlString(System.Xml.XmlReader reader)
        {
            return DataSetFromXmlString(reader, true);
        }

        public static DataSet DataSetFromXmlString(System.Xml.XmlReader reader, bool removeFirstElement)
        {
            //----------------------------------------------------------------------------------------------------------------------------
            // A Datatable/Dataset is persisted to an XML string like the following example.  When we persist a Datatable/Dataset to XML,
            // we add a root element so we can identify each parameter by name, so we must remove this extra element before rebuilding
            // our Dataset.
            //----------------------------------------------------------------------------------------------------------------------------
            //<DataSetName> 
            //   <DataTableName>
            //       <Column1>Val1</Column1>
            //       <Column2>Some value1</Column2>
            //   </DataTableName>
            //   <DataTableName>
            //       <Column1>Val2</Column1>
            //       <Column2>Some value2</Column2>
            //   </DataTableName>
            //</DataSetName>
            //----------------------------------------------------------------------------------------------------------------------------
            // Example usage from a client method -- the reader object we are passed in the code sample below will have an extra element 
            // at the beginning named <ColorsTable> which must be removed before we can properly deserialize the Datatable/Dataset.
            //
            //<ColorsTable>
            //   <DataSetName> 
            //      <DataTableName>
            //          <Column1>Val1</Column1>
            //          <Column2>Some value1</Column2>
            //      </DataTableName>
            //   </DataSetName>
            //</ColorsTable>
            //
            //----------------------------------------------------------------------------------------------------------------------------
            // If Not String.IsNullOrEmpty(parameters) Then
            //     Using xr As New Xml.XmlTextReader(New IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(parameters)))
            //         While xr.Read
            //             If xr.IsStartElement Then
            //                 Select Case xr.LocalName
            //                     Case Is = "ColorsTable"
            //                         Dim dt As DataTable = UserSettings.ReadDataTableElement(xr.ReadSubtree)
            //                 End Select
            //             End If
            //         End While
            //     End Using
            // End If
            //----------------------------------------------------------------------------------------------------------------------------
            DataSet result = null;
            try
            {
                if (removeFirstElement)
                {
                    if (reader.Read())
                    {
                        // Position should be on first element
                        if (reader.Read())
                        {
                            // Position should be on first child element, thereby removing the first element when 
                            // the ReadSubtree method is called.
                            DataSet ds = new DataSet();
                            ds.ReadXml(reader.ReadSubtree());
                            result = ds;
                        }
                    }

                }
                else
                {
                    DataSet ds = new DataSet();
                    ds.ReadXml(reader);
                    result = ds;
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return result;
        }

        public static string DataSetToXmlString(DataSet ds)
        {
            return DataSetToXmlString(ds, true);
        }

        public static string DataSetToXmlString(DataSet ds, bool includeXmlDeclaration)
        {
            var writerSettings = new System.Xml.XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = !includeXmlDeclaration;
            writerSettings.Indent = true;
            writerSettings.Encoding = System.Text.Encoding.ASCII;

            var sb = new System.Text.StringBuilder();
            using (var writer = System.Xml.XmlWriter.Create(sb, writerSettings))
            {
                ds.WriteXml(writer);
            }

            return sb.ToString();
        }


        #endregion

        #region DataTable Helper Methods


        public static DataTable DataTableFromXmlString(string serializedObject)
        {
            return DataTableFromXmlString(serializedObject, false);
        }

        public static DataTable DataTableFromXmlString(string serializedObject, bool removeFirstElement)
        {
            DataTable result = null;
            if (!string.IsNullOrEmpty(serializedObject))
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(serializedObject)))
                {
                    using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(ms))
                    {
                        result = DataTableFromXmlString(reader, removeFirstElement);
                    }
                }
            }
            return result;
        }

        public static DataTable DataTableFromXmlString(System.Xml.XmlReader reader)
        {
            return DataTableFromXmlString(reader, true);
        }

        public static DataTable DataTableFromXmlString(System.Xml.XmlReader reader, bool removeFirstElement)
        {
            DataTable dt = null;
            try
            {
                DataSet ds = DataSetFromXmlString(reader, removeFirstElement);
                if (ds != null)
                {
                    if (ds.Tables.Count > 0)
                    {
                        dt = ds.Tables[0];
                    }
                }
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return dt;
        }

        public static string DataTableToXmlString(DataTable dt)
        {
            return DataTableToXmlString(dt, true);
        }

        public static string DataTableToXmlString(DataTable dt, bool includeXmlDeclaration)
        {
            var writerSettings = new System.Xml.XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = !includeXmlDeclaration;
            writerSettings.Indent = true;
            writerSettings.Encoding = System.Text.Encoding.ASCII;

            var sb = new System.Text.StringBuilder();
            using (var writer = System.Xml.XmlWriter.Create(sb, writerSettings))
            {
                dt.WriteXml(writer);
            }

            return sb.ToString();
        }


        #endregion

        #region Object Helper Methods


        public static T Deserialize<T>(string serializedObject)
        {
            return Deserialize<T>(serializedObject, false);
        }

        public static T Deserialize<T>(string serializedObject, bool removeFirstElement)
        {
            T result = default(T);
            if (!string.IsNullOrEmpty(serializedObject))
            {
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.Text.Encoding.ASCII.GetBytes(serializedObject)))
                {
                    using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(ms))
                    {
                        result = Deserialize<T>(reader, removeFirstElement);
                    }
                }
            }
            return result;
        }

        public static T Deserialize<T>(System.Xml.XmlReader reader)
        {
            return Deserialize<T>(reader, false);
        }

        public static T Deserialize<T>(System.Xml.XmlReader reader, bool removeFirstElement)
        {
            T result = default(T);
            System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(T));
            if (removeFirstElement)
            {
                if (reader.Read())
                {
                    // Position should be on first element
                    if (reader.Read())
                    {
                        // Position should be on first child element, thereby removing the first element when 
                        // the ReadSubtree method is called.
                        if (xs.CanDeserialize(reader.ReadSubtree()))
                        {
                            result = (T)xs.Deserialize(reader.ReadSubtree());
                        }
                    }
                }

            }
            else if (xs.CanDeserialize(reader))
            {
                result = (T)xs.Deserialize(reader);
            }
            return result;
        }

        public static string Serialize<T>(T targetObject)
        {
            return Serialize<T>(targetObject, false);
        }

        public static string Serialize<T>(T targetObject, bool includeXmlDeclaration)
        {
            var writerSettings = new System.Xml.XmlWriterSettings();
            writerSettings.OmitXmlDeclaration = !includeXmlDeclaration;
            writerSettings.Indent = true;
            writerSettings.Encoding = System.Text.Encoding.ASCII;

            var sb = new System.Text.StringBuilder();
            using (var writer = System.Xml.XmlWriter.Create(sb, writerSettings))
            {
                // Clear the default namespace declarations
                var ns = new System.Xml.Serialization.XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);

                var serializer = new System.Xml.Serialization.XmlSerializer(targetObject.GetType());
                serializer.Serialize(writer, targetObject, ns);
            }

            return sb.ToString();
        }


        #endregion

    }
}
