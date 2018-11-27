namespace WebApplication
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Mail;
    using System.Text;

    public class CommunicationServiceProvider
    {
        private static SmtpClient _SmtpClient = null;
        private static SmtpClient SmtpClient
        {
            get
            {
                if (_SmtpClient == null)
                {
                    using (DAL.CraveatsDbContext craveatsDbContext = new DAL.CraveatsDbContext()) {
                        DAL.Settings instance = craveatsDbContext.Settings.FirstOrDefault();
                        if (instance != null) {
                            SmtpClient _iSmtpClient = new SmtpClient(instance.SMTPHost, instance.SMTPTLSPort ?? 0);
                            _iSmtpClient.EnableSsl = true;
                            _iSmtpClient.UseDefaultCredentials = true;
                            _iSmtpClient.Credentials = new NetworkCredential(instance.SMTPUserName, instance.SMTPPassword);
                            _SmtpClient = _iSmtpClient;
                        }
                    }
                }
                return _SmtpClient;
            }
        }

        private static MailAddress DefaultSender = new MailAddress("craveats@gmail.com", "Craveats Notification", Encoding.UTF8);
        private static MailAddress DefaultRecipient = new MailAddress("craveats@gmail.com");

        public static void SendOutgoingNotification(
            MailAddress toEmailAddress, string subject, string body)
        {
            try
            {
                MailMessage thisMessage = new MailMessage()
                {
                    Body = body,
                    From = DefaultSender,
                    IsBodyHtml = true,
                    Sender = DefaultSender,
                    Subject = subject,
                };

                thisMessage.ReplyToList.Add(toEmailAddress);
                thisMessage.To.Add(toEmailAddress);

                SendNotification(thisMessage);
            }
            catch (Exception e)
            {
                Trace.WriteLine(e);
            }
        }

        public static void SendAdminNotification(
            MailAddress fromEmailAddress, string subject, string body) {
            try
            {
                MailMessage thisMessage = new MailMessage() {
                    Body = body,
                    From = fromEmailAddress,
                    IsBodyHtml = true,
                    Sender = DefaultSender,
                    Subject = subject,
                };

                thisMessage.ReplyToList.Add(fromEmailAddress);
                thisMessage.To.Add(DefaultRecipient);

                SendNotification(thisMessage);
            }
            catch (Exception e) {
                Trace.WriteLine(e);
            }
        }

        public static void SendNotification(System.Net.Mail.MailMessage message, bool isProductionRelease = false)
        {
            try
            {
                if (message != null)
                {
                    StringBuilder sbRecipients = new StringBuilder();

                    if (!isProductionRelease)
                    {
                        MailAddressCollection recipients = new MailAddressCollection();

                        if (message.To.Count > 0 || message.Bcc.Count > 0 || message.CC.Count > 0)
                        {
                            foreach (MailAddress address in message.To)
                            {
                                sbRecipients.AppendFormat("{0}, ", address.ToString());
                            }
                            sbRecipients.Insert(0, "To: ");
                            sbRecipients.Remove(sbRecipients.Length - 1, 1);

                            message.To.Clear();
                            message.To.Add(DefaultRecipient);

                            sbRecipients.Insert(0, "; Bcc: ");
                            foreach (MailAddress address in message.Bcc)
                            {
                                sbRecipients.AppendFormat("{0}", address.ToString());
                            }
                            sbRecipients.Remove(sbRecipients.Length - 1, 1);
                            message.Bcc.Clear();

                            sbRecipients.Insert(0, "; CC: ");
                            foreach (MailAddress address in message.CC)
                            {
                                sbRecipients.AppendFormat("{0}", address.ToString());
                            }
                            sbRecipients.Remove(sbRecipients.Length - 1, 1);

                            sbRecipients.Append(";<br/>");
                            message.CC.Clear();

                            message.Body.Insert(0, sbRecipients.ToString());
                        }
                    }

                    if (message.Sender == null) {
                        message.Sender = DefaultSender;
                    }

                    SmtpClient.Send(message);
                }
            }
            catch (Exception e) {
                Trace.WriteLine(e);
            }
        }

    }

}