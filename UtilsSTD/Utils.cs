using System;
using System.Net;
using System.Net.Mail;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using System.Diagnostics;

namespace UtilsSTD
{
    public static class Utils
    {
        public static string MessageClass { set; get; }
        public static string Message { set; get; }
        public static string JSONResult { set; get; }
        public static HttpResponseMessage Response { set; get; }
        public static Exception Ex { set; get; }

        private static int result = 0;
        public static int Result
        {
            set
            {
                result = value;
                if (result == 0)
                {
                    MessageClass = "row bg-sucess";
                    Message = Response?.StatusCode.ToString();
                }
                else
                {
                    MessageClass = "row bg-danger";
                    Message = $"Http Response: {Response?.StatusCode}";
                    if (Ex != null)
                        Message += Ex.ToString();
                }
            }
            get
            {
                return result;
            }
        }


        public static string AlignPattern0(this string master, string current )
        {
            if ( master.Length > current.Length)
            {
                for(int i=0; i<current.Length; i++)
                {
                    if (current[i] != master[i])
                        if (i < master.Length - 1)
                            current = current.Insert(i, ".");
                        else
                            break;
                    if (current.Length == master.Length)
                        break;
                }
                if (current.Length < master.Length)
                {
                    var pad = "".PadRight(master.Length - current.Length, '.');
                    current = current + pad;
                }
            }
            return current;
        }
        public static string AlignPattern(this string master, string current)
        {
            if (master.Length > current.Length)
            {
                for (int i = 0; i < current.Length; i++)
                {
                    if (i<current.Length && i < (master.Length - 1) && current[i] != master[i])
                        //if (i < master.Length - 1)
                            current = current.Insert(i, ".");
                }
                if (current.Length < master.Length)
                {
                    var pad = "".PadRight(master.Length - current.Length, '.');
                    if (master[0] != current[0])
                        current = pad + current;
                    else current = current + pad;
                }
            }
            return current;
        }
        public static string AlignPattern2new(this string master, string current)
        {
            string newCurrent = "".PadLeft(master.Length, '.');
            int start = 0;
            int end = master.Length - 1;

            if (master[0] == current[0])
            {
                newCurrent = master[0] + "".PadLeft(master.Length, '.');
                start = 1;
            }

            if (master[master.Length - 1] == current[current.Length - 1])
            {
                newCurrent = newCurrent.Substring(0, master.Length - 1) + master[master.Length - 1];
                end = master.Length - 2;
            }


            for (int i = start; i < end; i++)
            {
                if (newCurrent[i] != master[i])
                    newCurrent = newCurrent.Substring(0,i) + master[i] + current.Substring(i+1);
                else
                    newCurrent = newCurrent.Substring(0, i) + current.Substring(i + 1);
            }
            return newCurrent;
        }
        public static string GetMedia(this string fileName)
        {
            string content = "";
            if (File.Exists(fileName))
            {
                var bytes = File.ReadAllBytes(fileName);
                var base64 = Convert.ToBase64String(bytes);
                content = string.Format("data:/image/jpg;base64,{0}", base64);
            }
            return content; 
        }

        public static int SaveStream(this MemoryStream ms, string fileName )
        {
            int result = 0;
            FileStream fileStr=null;
            
            try
            {
                if (!File.Exists(fileName))
                {
                    fileName.CreateDirectoryFromFileName();
                    fileStr = new FileStream(fileName, FileMode.Create, FileAccess.Write);
                    ms.WriteTo(fileStr);
                }
                else result = -2;
            }
            catch (Exception ex)
            {
                result = -1;
                Ex = ex;
                Message = ex.Message;
            }
            finally
            {
                fileStr?.Close();
            }
            return result;
        }

        public static async Task<string> GetMediaAsync(this string fileName)
        {
            string content = "";
            if (File.Exists(fileName))
            {
                var bytes = File.ReadAllBytes(fileName);
                var base64 = Convert.ToBase64String(bytes);
                content = string.Format("data:/image/jpg;base64,{0}", base64);
            }
            return content;
        }
        public static string ReadWholeFile(this string fileName )
        {
            string text = "";
            if (File.Exists(fileName))
                return File.ReadAllText(fileName);
            return text;
        }

        public static async Task<string> ReadFileAsync(this string fileName)
        {
            string content = "";
            if (File.Exists(fileName))
            {
                content = File.ReadAllText(fileName);
            }
            if (content.Contains("|"))
                content = content.Replace("|", "\r\n");

            while(content.Contains("\r\n\r\n"))
            {
                content = content.Replace("\r\n\r\n", "\r\n");
            }
            while( content.Contains("  "))
            {
                content = content.Replace("  ", " ");
            }
            return content;
        }


        private static void Init()
        {
            Message = "";
            Response = null;
            JSONResult = "";
            Ex = null;
        }

        public static void GenericSave<T>(this T obj, string fileName)
        {
            var JSONResult = System.Text.Json.JsonSerializer.Serialize<T>(obj);
            fileName.CreateDirectoryFromFileName();
            File.WriteAllText(fileName, JSONResult);
        }

        public static T GenericLoad<T>(ref T obj, string fileName)
        {
            Message = "File doesn-t exist.";
            if (File.Exists(fileName))
            {
                try
                {
                    var json = File.ReadAllText(fileName);
                    JSONDeserialize<T>(json, ref obj);
                    Message = "";
                }
                catch(Exception ex)
                {
                    Ex = ex;
                    Message = ex.Message;
                }
            }
            return obj;
        }


        public static T GenericLoad<T>(this  T obj, string fileName)
        {
            Message = "File doesn-t exist.";
            if (File.Exists(fileName))
            {
                try
                {
                    var json = File.ReadAllText(fileName);
                    JSONDeserializeNewtonSoft<T>(json, ref obj);
                    Message = "";
                }
                catch (Exception ex)
                {
                    Ex = ex;
                    Message = ex.Message;
                }
            }
            return obj;
        }


        public static void CreateDirectoryFromFileName(this string fileName)
        {
            string directory = Path.GetDirectoryName(fileName);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static void CreateDirectory(this string path)
        {
            if ( !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }


        public static string RemoveLast(this string text, string remove)
        {
            var text2 = text.Trim();
            if (text2.EndsWith(remove))
            {
                int index = text.LastIndexOf(remove);
                if (index >= 0)
                    text = text.Substring(0, index);
            }
            return text;
        }
        public static string Extract(this string text, string start, string end = "")
        {
            string result = "";
            if (string.IsNullOrEmpty(end))
                end = "\r\n";
            int index = text.ToLower().IndexOf(start.ToLower());
            if (index >= 0)
            {
                index += start.Length;
                int index2 = text.IndexOf(end.ToLower(), index);
                if (index2 > index)
                    result = text.Substring(index, index2 - index).Trim();
            }
            return result;
        }

        public static string GetYMDw(this DateTime dt)
        {
            return $"{dt.Year.ToString()}/{dt.Month.ToString().PadLeft(2, '0')}/{dt.Day.ToString().PadLeft(2, '0')} {dt.DayOfWeek.ToString().Substring(0, 3).ToUpper()}";
        }

        public static string GetHM(this DateTime dt)
        {
            return $"{dt.Hour.ToString().PadLeft(2, '0')}:{dt.Minute.ToString().PadLeft(2, '0')}";
        }

        public static string GetYYYYMM(this DateTime dt)
        {
            return dt.Year.ToString() + dt.Month.ToString().PadLeft(2, '0');
        }
        public static string DateString(this DateTime dt)
        {
            return $"{dt.Year.ToString().PadLeft(4, '0')}-{dt.Month.ToString().PadLeft(2, '0')}-{dt.Day.ToString().PadLeft(2, '0')}_{dt.Hour.ToString().PadLeft(2, '0')}-{dt.Minute.ToString().PadLeft(2, '0')}-{dt.Second.ToString().PadLeft(2, '0')}-{dt.Millisecond.ToString().PadLeft(3, '0')}";
        }

        public static string DateStringCompact(this DateTime dt)
        {
            return $"{dt.Year.ToString().PadLeft(4, '0')}{dt.Month.ToString().PadLeft(2, '0')}{dt.Day.ToString().PadLeft(2, '0')}.{dt.Hour.ToString().PadLeft(2, '0')}{dt.Minute.ToString().PadLeft(2, '0')}{dt.Second.ToString().PadLeft(2, '0')}{dt.Millisecond.ToString().PadLeft(3, '0')}.{dt.DayOfWeek.ToString().Substring(0, 3).ToUpper()}";
        }

        public static string DateStringYMDHM(this DateTime dt)
        {
            return $"{dt.Year.ToString().PadLeft(4, '0')}{dt.Month.ToString().PadLeft(2, '0')}{dt.Day.ToString().PadLeft(2, '0')}.{dt.Hour.ToString().PadLeft(2, '0')}{dt.Minute.ToString().PadLeft(2, '0')}.{dt.DayOfWeek.ToString().Substring(0, 3).ToUpper()}";
        }

        public static string GetMailTo(string fileName)
        {
            string mailto = "";
            if (File.Exists(fileName))
                mailto = File.ReadAllText(fileName);
            return mailto;
        }



        //send gmail
        //https://garrymarsland.com/sending-email-from-a-net-core-2-1/
        //YOU MUST PUT YOUR GMAIL ACCOUNT IN UNSECURE MODE TO ALLOW THROW MAILS

        public static void SendGMail(string from, string to, string subject, string body)
        {
            from = "tcsa52209@gmail.com";
            using (var message = new MailMessage())
            {
                message.To.Add(new MailAddress(to, "TCS RoboEngine"));
                message.From = new MailAddress(from, "TCS RoboEngine");
                //message.CC.Add(new MailAddress("cc@email.com", "CC Name"));
                //message.Bcc.Add(new MailAddress("bcc@email.com", "BCC Name"));
                message.Subject = subject;
                message.Body = body;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient("smtp.gmail.com"))
                {
                    client.Port = 587;
                    client.UseDefaultCredentials = false;
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = true;
                    client.Credentials = new NetworkCredential(from, "Esteeselpwd01");
                    client.Send(message);
                }
            }
        }

        public static void SendMail(string from, string to, string subject, string body)
        {
            /*
            EmailMessage m = new EmailMessage()
            {
                emailFrom = from,
                emailTo = to,
                emailSubject = subject,
                bodyHtml = body
            };

            string sm = System.Text.Json.JsonSerializer.Serialize<EmailMessage>(m);
            Post("http://matlkipceapp005:9014/prod/send-email/", sm);
            */
        }

        //async
        public async static void CallUrl(string url, string userName, string passwd, string json)
        {
            //var userName = "RoboEngine";
            //var passwd = "Pwcwelcome2";
            //var url = "https://uipath-unattended-dev.pwc.com/odata/Jobs/UiPath.Server.Configuration.OData.StartJobs";
            //url = "https://uipath-unattended-dev.pwc.com/api/account/authenticate";

            var client = new HttpClient();

            var authToken = Encoding.ASCII.GetBytes($"{userName}:{passwd}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(authToken));
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //await
            var result = await client.PostAsync(url, content);

            var content2 = await result.Content.ReadAsStringAsync();
            Console.WriteLine(content);
        }

        public static void PostUIPath(string url, string json)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                Credentials = new NetworkCredential("RoboEngine", "Pwcwelcome2", "uipath-unattended-dev.pwc.com"),
            };

            var httpClient = new HttpClient(httpClientHandler);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            //var result = await httpClient.GetString(url,json);
            var result = httpClient.PostAsync(url, content).Result;

            //return result;   

        }

        public static HttpResponseMessage Get(string url, string token = "")
        {
            var client = new HttpClient();

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            try
            {
                var result = client.GetAsync(url).Result;

                return result;   //.ToString();
            }
            catch (Exception ex)
            {
                string s = $"Error calling WEB Method:{ex.Message}";
                return null;

            }
        }


        //http request basic authentication
        //http://zetcode.com/csharp/httpclient/
        public static HttpResponseMessage Post(string url, string jsonString, string token = "")
        {
            var client = new HttpClient();

            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }

            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            try
            {
                var result = client.PostAsync(url, content).Result;

                return result;   //.ToString();
            }
            catch (Exception ex)
            {
                string s = $"Error calling WEB Method:{ex.Message}";
                return null;

            }
        }

        public static int SendMail(this string from, string password, string to, string subject, string text)
        {
            try
            {
                using (var mailMessage = new MailMessage())
                {
                    using (var client = new SmtpClient("smtp.gmail.com", 587))
                    {
                        //provide credentials
                        client.Credentials = new NetworkCredential(from, password);
                        client.EnableSsl = true;

                        // configure the mail message
                        mailMessage.From = new MailAddress("abcdefgh@gmail.com");
                        mailMessage.To.Insert(0, new MailAddress("abcdefgh@gmail.com"));
                        mailMessage.Subject = "Learn SMTP sending mail in .NET Core";
                        //mailMessage.Body = text;//"You did it ";
                        //  oMail.HtmlBody = "<font size=5>This is</font> <font color=red><b>a test</b></font>";


                        //send email
                        client.Send(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return 0;
        }


        public static string GetJSON(this string url)
        {
            Init();
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            Response = client.GetAsync(url).Result;
            if (Response.IsSuccessStatusCode)
                JSONResult = Response.Content.ReadAsStringAsync().Result;
            else
            {
                Result = -1;
                JSONResult = Result.ToString();
            }

            return JSONResult;

        }

        public static string PostJSON(this string url, string jsonString)
        {
            Init();
            var client = new HttpClient();
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
            try
            {
                Response = client.PostAsync(url, content).Result;
                if (Response.StatusCode == HttpStatusCode.OK)
                    JSONResult = Response.Content.ReadAsStringAsync().Result;
                else
                    Result = -2;
            }
            catch (Exception ex)
            {
                Ex = ex;
                Result = -3;
            }
            return JSONResult;
        }

        public static string JSONSerialize<T>(this T obj)
        {
            Init();
            try
            {
                JSONResult = System.Text.Json.JsonSerializer.Serialize<T>(obj);
            }
            catch (Exception ex)
            {
                Ex = ex;
                Result = -1;
            }
            //newtonsoft
            //JSONResult = JsonConvert.SerializeObject(obj );     //Formatting.Indented
            return JSONResult;
        }
        public static string NewtonSoftJSONSerialize<T>(this T obj)
        {
            Init();
            try
            {
                JSONResult = JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch (Exception ex)
            {
                Ex = ex;
                Result = -1;
            }
            return JSONResult;
        }

        public static T JSONDeserialize<T>(this string jsonString)
        {
            Init();
            try
            {
                var obj = System.Text.Json.JsonSerializer.Deserialize<T>(jsonString);
                return obj;
            }
            catch (Exception ex)
            {
                Ex = ex;
                Result = -2;
            }
            return default;
        }


        public static bool IsAlpha(this char ch)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".IndexOf(ch) >= 0;
        }


        public static int JSONDeserialize<T>(this string jsonString, ref T obj)
        {
            Init();
            try
            {
                obj = System.Text.Json.JsonSerializer.Deserialize<T>(jsonString);
            }
            catch (Exception ex)
            {
                Ex = ex;
                Result = -2;
            }
            return Result;
        }

        public static int JSONDeserializeNewtonSoft<T>(this string jsonString, ref T obj)
        {
            Init();
            try
            {
                obj = JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception ex)
            {
                Ex = ex;
                Result = -2;
            }
            return Result;
        }


        public static int JSONDeserializeList<T>(ref List<T> obj, string jsonString, bool checkEmptyList = false)
        {
            Init();
            try
            {
                obj = System.Text.Json.JsonSerializer.Deserialize<List<T>>(jsonString);
                if (checkEmptyList && obj != null && obj.Count == 0)
                {
                    //deserialize with newtonsoft
                    obj = JsonConvert.DeserializeObject<List<T>>(jsonString);
                }
            }
            catch (Exception ex)
            {
                Ex = ex;
                Result = -2;
            }
            return Result;
        }


        public static bool IsBitmap(this string s)
        {
            s = s.ToLower();
            return s.Contains(".bmp");
        }

        public static bool IsText(this string s)
        {
            s = s.ToLower();
            return s.Contains(".txt") || s.Contains(".text");
        }

        public static bool IsTiff(this string s)
        {
            s = s.ToLower();
            return s.Contains(".tiff") || s.Contains(".tiff");
        }

        public static bool IsImage(this string s)
        {
            s = s.ToLower();
            return s.Contains(".jpg") || s.Contains(".jpeg") ||
                s.Contains(".png") || s.Contains(".bmp") ||
                s.Contains(".webm");
        }
        public static bool IsAudio(this string s)
        {
            s = s.ToLower();
            return s.Contains(".mp3") || s.Contains(".wav") ||
                s.Contains(".acc") || s.Contains(".3gp") ||
                s.Contains(".amr") || s.Contains(".ogg") || s.Contains(".amr")
                || s.Contains(".wma");
        }


        public static bool IsJSON(this string s)
        {
            s = s.ToLower();
            return s.Contains(".json");
        }


        public static bool IsPDF(this string s)
        {
            s = s.ToLower();
            return s.Contains(".pdf");
        }


        public static bool IsDocument(this string ext)
        {
            ext = Path.GetExtension(ext).Trim().ToLower();
            return ".doc.docx.pdf.txt.text".IndexOf(ext.ToLower()) >= 0;
        }

        public static bool IsVideo(this string s)
        {
            s = s.ToLower();
            return s.Contains(".mp4") || s.Contains(".mpg") ||
                s.Contains(".mpeg") || s.Contains(".vob") || s.Contains(".avi")
                || s.Contains(".mov") || s.Contains(".wmv");
        }

        public static string convertImageToDisplay(this MemoryStream ms)
        {
            if (ms != null)
            {
                var imageArray = ms.ToArray();
                var base64 = Convert.ToBase64String(imageArray);
                return string.Format("data:/image/jpg;base64,{0}", base64);
            }
            return "";
        }
        public static string CleanPath(this string path)
        {
            path = path.Trim();
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                path += Path.DirectorySeparatorChar;
            return path;
        }

        public static string CapitalizeWords(this string text)
        {
            string newText = text;
            if (!string.IsNullOrEmpty(text) && text.Length > 1)
            {
                string[] words = text.Split(' ');
                newText = "";
                text = "";
                foreach (string word in words)
                {
                    string w = word.ToLower();
                    if (w.Length > 0)
                    {
                        string upper = w[0].ToString().ToUpper();
                        if (word.Length > 1)
                        {
                            text = $"{text}{upper}{w.Substring(1)} ";
                        }
                    }

                }
            }
            return text;
        }


        public static string Capitalize(this string text)
        {
            if (!string.IsNullOrEmpty(text) && text.Length > 1)
            {
                text = text.ToLower();
                text = text.Substring(0, 1).ToUpper() + text.Substring(1);
            }
            return text;
        }


        public static int ExecuteOSProcess(this string command, string workinDir, string parameters, bool createBat = false)
        {
            Init();
            try
            {

                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    Arguments = parameters,
                    FileName = command,
                    WorkingDirectory = workinDir
                };
                if (createBat)
                {
                    try
                    {
                        string batFileName = $"c:\\temp\\EXEC_{command}.bat";
                        batFileName.CreateDirectoryFromFileName();
                        File.WriteAllText(batFileName, $"cd {psi.WorkingDirectory}\r\n{command} {parameters}\r\n");
                    }
                    catch (Exception ex)
                    {

                    }
                }
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                var process = Process.Start(psi);
                process.WaitForExit();
                Result = process.ExitCode;
            }
            catch (Exception ex)
            {
                Ex = ex;
                Message = ex.Message;
                Result = -99;
            }
            return Result;
        }

        public static int ExecuteProcessNoWait(this string command, string fileName, bool createBat = false)
        {
            Init();
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    Arguments = $"{Path.GetFileName(fileName)}",
                    FileName = command,
                    WorkingDirectory = Path.GetDirectoryName(fileName)
                };
                if (createBat)
                {
                    try
                    {
                        string batFileName = $"c:\\temp\\EXEC_{command}.bat";
                        batFileName.CreateDirectoryFromFileName();
                        File.WriteAllText(batFileName, $"cd {psi.WorkingDirectory}\r\n{command} {fileName}\r\n");
                    }
                    catch (Exception ex)
                    {

                    }
                }
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                //string bat = $"cd {Path.GetDirectoryName(filEName)}\r\nTESSERACT ";

                var process = Process.Start(psi);
                //process.WaitForExit();
                Result = process.ExitCode;
            }
            catch (Exception ex)
            {
                Ex = ex;
                Message = ex.Message;
                Result = -1;
            }
            return Result;
        }

        public static int ExecuteProcess(this string command, string fileName, bool createBat = false)
        {
            Init();
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo() 
                { 
                    Arguments = $"{Path.GetFileName(fileName)} {Path.GetFileName(fileName)}", 
                    FileName = command, 
                    WorkingDirectory = Path.GetDirectoryName(fileName) 
                };
                if ( createBat )
                {
                    try
                    {
                        string batFileName = $"c:\\temp\\EXEC_{command}.bat";
                        batFileName.CreateDirectoryFromFileName();
                        File.WriteAllText(batFileName, $"cd {psi.WorkingDirectory}\r\n{command} {fileName}\r\n");
                    }
                    catch(Exception ex)
                    {

                    }
                }
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                //string bat = $"cd {Path.GetDirectoryName(filEName)}\r\nTESSERACT ";

                var process = Process.Start(psi);
                process.WaitForExit();
                Result = process.ExitCode;
            }
            catch(Exception ex)
            {
                Ex = ex;
                Message = ex.Message;
                Result = -1;
            }
            return Result;
        }


        public static int ExecuteProcess(this string command, string arguments, string startFolder, bool createBat = false)
        {
            Init();
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    Arguments = arguments,
                    FileName = command,
                    WorkingDirectory = startFolder
                };
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                if (createBat)
                {
                    try
                    {
                        string batFileName = $"c:\\temp\\EXEC_{command}.bat";
                        batFileName.CreateDirectoryFromFileName();
                        File.WriteAllText(batFileName, $"cd {psi.WorkingDirectory}\r\n{command} {arguments}\r\n");
                    }
                    catch (Exception ex)
                    {

                    }
                }
                var process = Process.Start(psi);
                process.WaitForExit();
                Result = process.ExitCode;
            }
            catch (Exception ex)
            {
                Ex = ex;
                Message = ex.Message;
                Result = -1;
            }
            return Result;
        }

        
        public static string CleanText(this string text, string cleans)
        {
            var cleanTexts = cleans.Split(',');
            text = text.Trim();
            foreach(var ct in cleanTexts)
            {
                while( text.Contains(ct))
                    text = text.Replace(ct, "");
            }
            return text;
        }


    }
}

