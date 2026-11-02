using System;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;




/**
 * Name: Trung Kien Doan
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 * **/


namespace ConsoleApp1
{


    public class Submission
    {
        public static string xmlURL = "https://kiendoan2005.github.io/CSE445_a4/NationalParks.xml";
        public static string xmlErrorURL = "https://kiendoan2005.github.io/CSE445_a4/NationalParksErrors.xml";
        public static string xsdURL = "https://kiendoan2005.github.io/CSE445_a4/NationalParks.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);


            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);


            result = Xml2Json(xmlURL);
            Console.WriteLine(result);

        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            //return "No Error" if XML is valid. Otherwise, return the desired exception message.
            string errors = "";
            try
            {
                // Download the XML and XSD content from the given URLs

                // Set up the XmlReaderSettings with the XSD schema for validation
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema; // Set the validation type to Schema
                settings.Schemas.Add(null, xsdUrl); // Load the XSD schema directly
                settings.ValidationEventHandler += (sender, e) =>
                {
                    errors += e.Severity + ": " + e.Message + " (Line " + e.Exception?.LineNumber + ")\n"; // Collect validation errors
                };
                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { } // Read through the XML document to trigger validation
                };
            }
            catch (XmlException ex) { errors += "XML Error: " + ex.Message + "\n"; } // Catch any XML exceptions and add to errors.
            catch (Exception ex) { errors += "Error: " + ex.Message + "\n"; } // Catch any exceptions and add to errors.
            return errors == "" ? "No errors are found" : errors; // Return "No errors are found" if no errors, otherwise return the collected errors.
        }

        public static string Xml2Json(string xmlUrl)
        {
            // The returned jsonText needs to be de-serializable by Newtonsoft.Json package. (JsonConvert.DeserializeXmlNode(jsonText))
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlUrl); // Load the XML document from the given URL
            
            // Remove the XML declaration if it exists
            if (xmlDoc.FirstChild is XmlDeclaration)
            {
                xmlDoc.RemoveChild(xmlDoc.FirstChild); // Remove the XML declaration to avoid issues during conversion
            };

            // Remove all comments from the XML document
            XmlNodeList comments = xmlDoc.SelectNodes("//comment()"); // Select all comment nodes in the XML document
            foreach (XmlNode comment in comments)
            {
                comment.ParentNode.RemoveChild(comment); // Remove each comment node from its parent
            }

            // Remove root attributes if they exist (schemaLocation)
            xmlDoc.DocumentElement.Attributes.RemoveAll(); // Remove all attributes from the root element to simplify the JSON structure


            string json = JsonConvert.SerializeXmlNode(xmlDoc, Newtonsoft.Json.Formatting.Indented); // Convert the XML document to JSON format with indentation for readability
            // Wrap any single Phone string into an array so every park has a Phone array
            JObject phone = JObject.Parse(json); // Parse the JSON string into a JObject for manipulation
            foreach(JToken park in phone["NationalParks"]["NationalPark"])
            {
                JToken phonetoken = park["Phone"]; // Get the Phone token for the current park
                if (phonetoken != null && phonetoken.Type != JTokenType.Array) { park["Phone"] = new JArray(phonetoken);  }
                // If the Phone token exists and is not already an array, wrap it in a JArray to ensure consistency across all parks
            }




            return phone.ToString(Newtonsoft.Json.Formatting.Indented); // Convert the XML document to JSON format and return it
        }

        // Helper method to download content from URL
        private static string DownloadContent(string url)
        {
            using (System.Net.WebClient client = new System.Net.WebClient())
            {
                return client.DownloadString(url);
            }
        }
    }

}