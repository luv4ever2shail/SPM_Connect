﻿using SearchDataSPM.Miscellaneous;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace SearchDataSPM
{
    internal class ErrorHandler
    {
        public void EmailExceptionAndActionLogToSupport(object sender, Exception exception)
        {
            StringBuilder _errMsg = new StringBuilder();
            List<string> _emailAttachments = new List<string>();
            //HTML table containing the Exception details for support email
            _errMsg.Append("User: ");
            _errMsg.Append(Environment.UserName).Append('\n');
            _errMsg.Append("Time: ");
            DateTime datecreated = DateTime.Now;
            string sqlFormattedDatetime = datecreated.ToString("yyyy-MM-dd HH:mm:ss");
            _errMsg.Append(sqlFormattedDatetime).Append('\n');
            _errMsg.Append("Exception Type: ");
            _errMsg.Append(sender.ToString()).Append(" Exception\n");

            if (exception != null)
            {
                _errMsg.Append("Message: ");
                _errMsg.Append(exception.Message.Replace(" at ", " at "));
                _errMsg.Append("\n");
                if (exception.InnerException != null)
                {
                    _errMsg.Append("Inner Exception: ");
                    _errMsg.Append(exception.InnerException.Message).Append('\n');
                }
                _errMsg.Append("Stacktrace: ");
                _errMsg.Append(exception.StackTrace).Append('\n');
            }

            //Write out the logs in memory to file

            //Get list of today's log files

            //Adding a screenshot of the broken window for support is a good touch
            _emailAttachments.Add(Screenshot.TakeScreenshotReturnFilePath());
            SendEmail("shail@spm-automation.com", "SPM CONNECT - PROBLEM CASE ID: " + Path.GetRandomFileName(), _errMsg.ToString(), _emailAttachments.ToArray());
        }

        public void SendEmail(string emailTo, string emailSubject, string emailBody, string[] emailFileAttachments)
        {
            MailMessage message = new MailMessage();
            SmtpClient SmtpServer = new SmtpClient("spmautomation-com0i.mail.protection.outlook.com");
            message.From = new MailAddress("connect@spm-automation.com", "SPM Connect");
            System.Net.Mail.Attachment attachment;
            emailTo = emailTo.TrimEnd(';', ',');
            string[] emailArr = emailTo.Split(';', ',');
            foreach (string email in emailArr)
            {
                if (!string.IsNullOrEmpty(email))
                {
                    message.To.Add(email);
                }
            }

            message.Subject = emailSubject;

            if (emailFileAttachments != null)
            {
                foreach (string fileAttachment in emailFileAttachments)
                {
                    if (!string.IsNullOrEmpty(fileAttachment))
                    {
                        attachment = new System.Net.Mail.Attachment(fileAttachment);
                        message.Attachments.Add(attachment);
                    }
                }
            }

            message.Body = emailBody;

            //Save the email message to the Drafts folder (where it can be retrieved, updated, and sent at a later time).

            try
            {
                SmtpServer.Port = 25;
                SmtpServer.UseDefaultCredentials = true;
                SmtpServer.EnableSsl = true;
                SmtpServer.Send(message);
                //System.Windows.Forms.MessageBox.Show("SPM Connect ran into error!! Error report has been sent - PLEASE CONTACT " + "shail@spm-automation.com" + "!", "Email sending Success", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                //System.Windows.Forms.MessageBox.Show("FAILED TO SEND EMAIL - PLEASE CONTACT " + "shail@spm-automation.com" + "!", "Email sending failed", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
            }
        }
    }
}