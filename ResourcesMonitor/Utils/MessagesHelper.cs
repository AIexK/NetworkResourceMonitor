using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using ResourcesMonitor.Utils;
using OpenPop.Pop3;
using OpenPop.Mime;

namespace ResourcesMonitor
{
    public class MessagesHelper
    {
        public static string SendGetRequest(string url)
        {
            var request = System.Net.WebRequest.Create(url);
            var response = request.GetResponse();
            var stream = response.GetResponseStream();
            var streamReader = new System.IO.StreamReader(stream);
            var @out = streamReader.ReadToEnd();
            streamReader.Close();
            return @out;
        }

        private static object _lock = new object();

   
        public static void SendTelegramMessage(string messageText)
        {
#if !DEBUG
            SendGetRequest($"{OptionsHelper.Options.TelegramRequest}{messageText}");
#endif            
        }

        public static void SendMailText(
            IEnumerable<string> recipients,
            IEnumerable<string> cc,
            IEnumerable<string> bcc,
            string subject,
            string htmlBodyText,
            List<string> attachmentsFullPath,
            MailPriority priority = MailPriority.Normal)
        {
            try
            {
                bool simpleText = false;
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(Constants.InfoAccauntData.MAIL, "no-reply");
                    recipients.Each(x => message.To.Add(new MailAddress(x)));
                    if (cc != null)
                    {
                        cc.Each(x => message.CC.Add(new MailAddress(x)));
                    }
                    if (bcc != null)
                    {
                        bcc.Each(x => message.Bcc.Add(new MailAddress(x)));
                    }
                    message.Subject = subject;
                    message.IsBodyHtml = !simpleText;
                    message.SubjectEncoding = System.Text.Encoding.UTF8;
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.Body = htmlBodyText;
                    message.Priority = priority;

                    AlternateView htmlview = default(AlternateView);
                    if (simpleText)
                    {
                        htmlview = AlternateView.CreateAlternateViewFromString(message.Body, null, MediaTypeNames.Text.Plain);
                    }
                    else
                    {
                        htmlview = AlternateView.CreateAlternateViewFromString(message.Body, System.Text.Encoding.Unicode, "text/html");
                    }

                    message.AlternateViews.Add(htmlview);

                    foreach (var attachmentFullPath in attachmentsFullPath)
                    {
                        Attachment data = new Attachment(attachmentFullPath, MediaTypeNames.Application.Octet);
                        message.Attachments.Add(data);
                    }

                    using (SmtpClient client = new SmtpClient
                    {
                        Host = Constants.InfoAccauntData.HOST,
                        Port = Constants.InfoAccauntData.SMTP_PORT,
                        Credentials = new NetworkCredential(
                                Constants.InfoAccauntData.MAIL,
                                Constants.InfoAccauntData.PASSWORD),
                    })

                    {
                        client.Send(message);                       
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Sending mail error! Message: {ex.Message}");
                throw new Exception(ex.Message, ex);
            }
        }
    }
}