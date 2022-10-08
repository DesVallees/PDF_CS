using iTextSharp.text.pdf;
using Document = iTextSharp.text.Document;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using WordDoc = Spire.Doc;

var dataDir = @"c:\Users\sntgo\Xamarin\PDFEditor\";

var information = new Dictionary<string, string>();

information["FirstName"] = "Santiago";
information["MiddleName"] = "";
information["LastName"] = "Ovalles";
information["Email"] = "sntg.ovalde@gmail.com";
information["Date"] = DateTime.Now.ToString("MM/dd/yyyy");
information["LawyerFirstName"] = "Katherine";
information["LawyerMiddleName"] = "";
information["LawyerLastName"] = "Canto";
if (information["MiddleName"].Length > 0)
{
    information["FullName"] = information["FirstName"] + ' ' + information["MiddleName"] + ' ' + information["LastName"];
}
else
{
    information["FullName"] = information["FirstName"] + ' ' + information["LastName"];
}

string informationKeys = "";
foreach (var key in information)
{
    informationKeys += key.Key + ", ";
}
informationKeys = informationKeys.Remove(informationKeys.Length - 2, 2);

StreamReader formsMapString = new StreamReader(@"C:\Users\sntgo\Xamarin\PDFEditor\formsMap.json");
dynamic formsMap = JsonConvert.DeserializeObject(formsMapString.ReadToEnd());
formsMapString.Close();
formsMapString.Dispose();

void enumerateFields(string formName)
{
    string pdfTemplate = dataDir + formName + ".pdf";
    var pdfResult = dataDir + formName + "-enum" + ".pdf";


    File.Delete(pdfResult);
    FileStream doc = new FileStream(pdfResult, FileMode.OpenOrCreate);
    PdfReader pdfReader = new PdfReader(pdfTemplate);
    PdfReader.unethicalreading = true;
    PdfStamper pdfStamper = new PdfStamper(pdfReader, doc);
    AcroFields pdfFormFields = pdfStamper.AcroFields;
    int i = 0;
    foreach (var field in pdfReader.AcroFields.Fields)
    {
        string name = field.Key.ToString();
        pdfFormFields.SetField(name, i.ToString());
        try
        {
            if(pdfFormFields.GetAppearanceStates(name)[1] == "Off")
            {
                Console.WriteLine(i.ToString() + " is a checkbox and its value is: " + pdfFormFields.GetAppearanceStates(name)[0]);
                pdfFormFields.SetField(name, pdfFormFields.GetAppearanceStates(name)[0]);
            }
        }
        catch (Exception ex)
        {}
        i++;
    }

    pdfStamper.FormFlattening = false;

    if (pdfStamper != null)
    {
        pdfStamper.Close();
        pdfStamper.Dispose();
    }

    if (pdfReader != null)
    {
        pdfReader.Close();
        pdfReader.Dispose();
    }

    if (doc != null)
    {
        doc.Close();
        doc.Dispose();

    }

    var p = new Process();
    p.StartInfo = new ProcessStartInfo(pdfResult)
    {
        UseShellExecute = true
    };
    p.Start();
}

void fillForm(string formName)
{
    string pdfTemplate = dataDir + formName + ".pdf";
    var pdfResult = dataDir + formName + "-formatted" + ".pdf";
    

    File.Delete(pdfResult);
    FileStream doc = new FileStream(pdfResult, FileMode.OpenOrCreate);
    PdfReader pdfReader = new PdfReader(pdfTemplate);
    PdfReader.unethicalreading = true;
    PdfStamper pdfStamper = new PdfStamper(pdfReader, doc);
    AcroFields pdfFormFields = pdfStamper.AcroFields;

    foreach (KeyValuePair<string, string> entry in information)
    {
        var fields = formsMap[formName][entry.Key];
        foreach(var i in fields)
        {
            pdfFormFields.SetField(i.ToString(), entry.Value);
        }
    }

    pdfStamper.FormFlattening = false;

    if (pdfStamper != null)
    {
        pdfStamper.Close();
        pdfStamper.Dispose();
    }

    if (pdfReader != null)
    {
        pdfReader.Close();
        pdfReader.Dispose();
    }

    if (doc != null)
    {
        doc.Close();
        doc.Dispose();

    }

    var p = new Process();
    p.StartInfo = new ProcessStartInfo(pdfResult)
    {
        UseShellExecute = true
    };
    p.Start();
}
void addField(string formName, string category, string fieldNumber)
{
    string pdfTemplate = dataDir + formName + ".pdf";

    PdfReader pdfReader = new PdfReader(pdfTemplate);
    PdfReader.unethicalreading = true;

    int i = 0;
    foreach (var field in pdfReader.AcroFields.Fields)
    {
        if(i.ToString() == fieldNumber)
        {
            string name = field.Key.ToString();
            formsMap[formName][category].Add(name);
            string mapString = JsonConvert.SerializeObject(formsMap);
            File.WriteAllText(@"C:\Users\sntgo\Xamarin\PDFEditor\formsMap.json", mapString);
        }
        i++;
    }

    if (pdfReader != null)
    {
        pdfReader.Close();
        pdfReader.Dispose();
    }
}
void removeField(string formName, string fieldNumber)
{
    string pdfTemplate = dataDir + formName + ".pdf";

    PdfReader pdfReader = new PdfReader(pdfTemplate);
    PdfReader.unethicalreading = true;

    int i = 0;
    foreach (var field in pdfReader.AcroFields.Fields)
    {
        if (i.ToString() == fieldNumber)
        {
            string name = field.Key.ToString();
            var propertyList = (JObject)formsMap[formName];
            foreach (var key in propertyList)
            {
                string category = key.Key;
                List<string> miniArray = formsMap[formName][category].ToObject<List<string>>();
                miniArray.Remove(name);
                formsMap[formName][category] = JArray.FromObject(miniArray);
            }
            string mapString = JsonConvert.SerializeObject(formsMap);
            File.WriteAllText(@"C:\Users\sntgo\Xamarin\PDFEditor\formsMap.json", mapString);
        }
        i++;
    }

    if (pdfReader != null)
    {
        pdfReader.Close();
        pdfReader.Dispose();
    }
}
bool MergePDFs(List<string> fileNames, string targetPdf)
{
    bool merged = true;

    File.Delete(targetPdf);
    using (FileStream stream = new FileStream(targetPdf, FileMode.OpenOrCreate))
    {
        Document document = new Document();
        PdfCopy pdf = new PdfCopy(document, stream);
        PdfReader reader = null;
        PdfReader.unethicalreading = true;
        try
        {
            document.Open();
            foreach (string file in fileNames)
            {
                reader = new PdfReader(file);
                pdf.AddDocument(reader);
                reader.Close();
                Console.WriteLine(file + "merged into" + targetPdf + "successfully...");
            }
        }
        catch (Exception ex)
        {
            merged = false;
            Console.WriteLine("Error while trying to merge files: " + ex.Message);
            Console.WriteLine();
            if (reader != null)
            {
                reader.Close();
            }
        }
        finally
        {
            if (document != null)
            {
                try
                {
                    document.Close();
                }
                catch
                {}
            }
        }
    }
    return merged;
}
bool ConvertToPDF(string source, string output)
{
    try
    {
        WordDoc.Document document = new WordDoc.Document();
        document.LoadFromFile(source);
        document.SaveToFile(output, WordDoc.FileFormat.PDF);

        return true;
    }
    catch(Exception ex)
    {
        Console.WriteLine(ex.Message);
        return false;
    }
}



Console.WriteLine("INPUT 'r' AT ANY POINT TO RESTART \nDON'T WRITE FILE EXTENTIONS ('.pdf', '.docx', etc.)");
Console.WriteLine();
while (true)
{
    Console.Write(" - Fill form (f)\n - Define field answer (d)\n - Delete field answer (e)\n - Merge PDFs (m)\n - Convert file to PDF (c)\n\n Action: ");
    string action = Console.ReadLine();
    Console.WriteLine();
    if (action == "r")
    {
        Console.WriteLine();
        continue;
    }
    else if (action == "f")
    {
        Console.Write("Form's name: ");
        string formName = Console.ReadLine();
        if (formName == "r")
        {
            Console.WriteLine();
            continue;
        }
        try
        {
            fillForm(formName);
            Console.WriteLine("Form Filled Successfully");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while trying to fill form: " + ex.Message);
            Console.WriteLine();
        }
    }else if (action == "d")
    {
        Console.Write("Form's name: ");
        string formName = Console.ReadLine();
        if (formName == "r")
        {
            Console.WriteLine();
            continue;
        }
        Console.Write("Open form with fields enumerated? (y/n): ");
        string response = Console.ReadLine();
        if (response == "r")
        {
            Console.WriteLine();
            continue;
        }
        if (response == "y")
        {
            try
            {
                enumerateFields(formName);
                Console.WriteLine("Form opened with fields enumerated...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while trying to open form with fields enumerated: " + ex.Message);
                Console.WriteLine();
                continue;
            }
        }
        Console.WriteLine();
        Console.Write("Number of the field to edit: ");
        string fieldNumber = Console.ReadLine();
        if (fieldNumber == "r")
        {
            Console.WriteLine();
            continue;
        }
        Console.Write("What information should be used to fill this field (" + informationKeys + "): ");
        string category = Console.ReadLine();
        if (category == "r")
        {
            Console.WriteLine();
            continue;
        }
        try
        {
            addField(formName, category, fieldNumber);
            Console.WriteLine("Field added successfully");
            Console.WriteLine(); 
            Console.Write("Open Updated Form? (y/n): ");
            string lastResponse = Console.ReadLine();
            if (lastResponse == "y")
            {
                try
                {
                    fillForm(formName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while trying to open form: " + ex.Message);
                    Console.WriteLine();
                    continue;
                }
            }
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while adding field: " + ex.Message);
            Console.WriteLine();
        }
    }
    else if (action == "e")
    {
        Console.Write("Form's name: ");
        string formName = Console.ReadLine();
        if (formName == "r")
        {
            Console.WriteLine();
            continue;
        }
        Console.Write("Open form with fields enumerated? (y/n): ");
        string response = Console.ReadLine();
        if (response == "r")
        {
            Console.WriteLine();
            continue;
        }
        if (response == "y")
        {
            try
            {
                enumerateFields(formName);
                Console.WriteLine("Form opened with fields enumerated...");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while trying to open form with fields enumerated: " + ex.Message);
                Console.WriteLine();
                continue;
            }
        }
        Console.WriteLine();
        Console.Write("Number of the field to clean: ");
        string fieldNumber = Console.ReadLine();
        if (fieldNumber == "r")
        {
            Console.WriteLine();
            continue;
        }
        try
        {
            removeField(formName, fieldNumber);
            Console.WriteLine("Field removed successfully");
            Console.WriteLine();
            Console.Write("Open Updated Form? (y/n): ");
            string lastResponse = Console.ReadLine();
            if (lastResponse == "y")
            {
                try
                {
                    fillForm(formName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while trying to open form: " + ex.Message);
                    Console.WriteLine();
                    continue;
                }
            }
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error while removing field: " + ex.Message);
            Console.WriteLine();
        }
    }
    else if (action == "m")
    {
        bool adding = true;
        List<string> files = new List<string>();
        while (adding)
        {
            Console.Write("PDF's name (" + (files.Count + 1) + "): ");
            string docName = Console.ReadLine();
            if (docName == "r")
            {
                files.Clear();
                adding = false;
            }
            files.Add(dataDir + docName + ".pdf");
            if (files.Count != 1)
            {
                Console.Write("Add another file (y/n): ");
                string ans = Console.ReadLine();
                if (ans == "n") { adding = false; }
            }
        }
        if (files.Count == 0 || files.Count == 1)
        {
            Console.WriteLine();
            continue;
        }
        
        Console.Write("Output file's name: ");
        string outputName = Console.ReadLine();
        if (outputName == "r")
        {
            Console.WriteLine();
            continue;
        }
        outputName = dataDir + outputName + ".pdf";
        bool mergeSuccessful = MergePDFs(files, outputName);
        if (mergeSuccessful)
        {
            Console.WriteLine("Merge has succeded!");
            Console.Write("Open output file? (y/n): ");
            string lastResponse = Console.ReadLine();
            Console.WriteLine();
            if (lastResponse == "y")
            {
                try
                {
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(outputName)
                    {
                        UseShellExecute = true
                    };
                    p.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while trying to open PDF: " + ex.Message);
                    Console.WriteLine();
                }
            }
        }
    }
    else if (action == "c")
    {
        Console.Write("File's name: ");
        string name = Console.ReadLine();
        if (name == "r")
        {
            Console.WriteLine();
            continue;
        }
        Console.Write("Current file's extention ('doc', 'docx', etc.): ");
        string extention = Console.ReadLine();
        if (extention == "r")
        {
            Console.WriteLine();
            continue;
        }
        string source = dataDir + name + "." + extention;
        string output = dataDir + name + ".pdf";
        bool convertionSuccessful = ConvertToPDF(source, output);
        Console.WriteLine();
        if (convertionSuccessful)
        {
            Console.WriteLine("Convertion succeded!");
            Console.Write("Open output file? (y/n): ");
            string lastResponse = Console.ReadLine();
            Console.WriteLine();
            if (lastResponse == "y")
            {
                try
                {
                    var p = new Process();
                    p.StartInfo = new ProcessStartInfo(output)
                    {
                        UseShellExecute = true
                    };
                    p.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while trying to open file: " + ex.Message);
                    Console.WriteLine();
                }
            }
        }
    }
    else
    {
        Console.WriteLine("Invalid Action");
        Console.WriteLine();
    }
}
