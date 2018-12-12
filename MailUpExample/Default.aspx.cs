using MailUpExample.Entity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;

namespace MailUpExample {
    public partial class _Default : System.Web.UI.Page {
        protected void Page_Load(object sender, EventArgs e) {
            // This "code" parameter is used when called back from MailUp logon. 
            // Need to complete logging on by retreiving the access token.
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];
            if (Request.Params["code"] != null) {
                if (mailUp != null && mailUp.AccessToken == null) {
                    string token = mailUp.RetrieveAccessToken(Request.Params["code"]);
                }
            }
            if (mailUp != null && mailUp.AccessToken != null) {
                pAuthorization.InnerText = "Authorized. Token: " + mailUp.AccessToken;
            }
            else {
                pAuthorization.InnerText = "Unauthorized";
            }

            if (!IsPostBack) {
                lstVerb.DataSource = new string[] { "GET", "POST", "PUT", "DELETE" };
                lstVerb.DataBind();

                lstContentType.DataSource = new string[] { "JSON", "XML" };
                lstContentType.DataBind();

                lstEndpoint.DataSource = new string[] { "Console", "MailStatistics" };
                lstEndpoint.DataBind();
            }
        }
        // Sign in button - redirects to MailUp Logon page.
        protected void LogOn_ServerClick(object sender, EventArgs e) {
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];
            if (mailUp != null) mailUp.LogOn();
        }
        // Sign in button - get tokens with password flow.
        protected void LogOnWithUsernamePassword_ServerClick(object sender, EventArgs e) {
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];
            if (mailUp != null) mailUp.LogOnWithUsernamePassword(txtUsr.Text, txtPwd.Text);
            if (mailUp != null && mailUp.AccessToken != null) {
                pAuthorization.InnerText = "Authorized. Token: " + mailUp.AccessToken;
            }
            else {
                pAuthorization.InnerText = "Unauthorized";
            }
        }
        // Call method button - calls a single API method.
        protected void CallMethod_ServerClick(object sender, EventArgs e) {
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];
            try {
                if (mailUp != null) {
                    string resourceURL = "" + (lstEndpoint.SelectedValue == "Console" ? mailUp.ConsoleEndpoint + txtPath.Text : mailUp.MailstatisticsEndpoint + txtPath.Text);
                    string strResult = mailUp.CallMethod(resourceURL, lstVerb.SelectedValue, txtBody.Text, lstContentType.SelectedValue == "JSON" ? MailUp.ContentType.Json : MailUp.ContentType.Xml);
                    pResultString.InnerText = txtPath.Text + " returned: " + strResult;
                }
            }
            catch (MailUp.MailUpException ex) {
                pResultString.InnerText = "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode;
            }
        }
        // EXAMPLE 1 - IMPORT RECIPIENTS INTO NEW GROUP
        protected void RunExample1_ServerClick(object sender, EventArgs e) {
            string status = "";
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];
            try {
                if (mailUp != null) {
                    // List ID = 1 is used in all example calls
                    string resourceURL = "";
                    string strResult = "";
                    object objResult;
                    Dictionary<string, object> items = new Dictionary<string, object>();

                    // Given a default list id (use idList = 1), request for user visible groups
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Groups";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    items = (Dictionary<string, object>)objResult;
                    object[] groups = (object[])items["Items"];
                    int groupId = -1;
                    foreach (Dictionary<string, object> group in groups) {
                        object name = group["Name"];
                        if ("test import".Equals(name)) groupId = int.Parse(group["idGroup"].ToString());
                    }

                    status += $"<p>Given a default list id (use idList = 1), request for user visible groups<br/>{"GET"} {resourceURL} - OK</p>";

                    // If the list does not contain a group named “test import”, create it
                    if (groupId == -1) {
                        groupId = 100;
                        resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Group";
                        string groupRequest = "{\"Deletable\":true,\"Name\":\"test import\",\"Notes\":\"test import\"}";
                        strResult = mailUp.CallMethod(resourceURL, "POST", groupRequest, MailUp.ContentType.Json);
                        objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                        items = (Dictionary<string, object>)objResult;
                        groups = (object[])items["Items"];
                        foreach (Dictionary<string, object> group in groups) {
                            object name = group["Name"];
                            if ("test import".Equals(name))
                                groupId = int.Parse(group["idGroup"].ToString());
                        }
                    }
                    Session["groupId"] = groupId;

                    status += $"<p>If the list does not contain a group named “test import”, create it<br/>{"POST"} {resourceURL} - OK</p>";

                    // Request for dynamic fields to map recipient name and surname
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/Recipient/DynamicFields";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    status += $"<p>Request for dynamic fields to map recipient name and surname<br/>{"GET"} {resourceURL} - OK</p>";

                    // Import recipients to group
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/Group/" + groupId + "/Recipients";
                    string recipientRequest = "[{\"Email\":\"test@test.test\",\"Fields\":[{\"Description\":\"String description\",\"Id\":1,\"Value\":\"String value\"}]," +
                        "\"MobileNumber\":\"\",\"MobilePrefix\":\"\",\"Name\":\"John Smith\"}]";
                    strResult = mailUp.CallMethod(resourceURL, "POST", recipientRequest, MailUp.ContentType.Json);
                    int importId = int.Parse(strResult);
                    status += $"<p>Import recipients to group.<br />{"GET"} {resourceURL} - OK<p>";

                    // Check the import result
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/Import/" + importId;
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    status += $"<p>Check the import result.<br />{"GET"} {resourceURL} - OK<p>";
                    status += "<p>Example methods completed successfully<p>";
                }
            }
            catch (MailUp.MailUpException ex) {
                status += "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode + "<br/>";
            }

            pExampleResultString.InnerHtml = status;
        }
        // EXAMPLE 2 - UNSUBSCRIBE A RECIPIENT FROM A GROUP
        protected void RunExample2_ServerClick(object sender, EventArgs e) {
            string status = "";
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];

            try {
                if (mailUp != null) {
                    // List ID = 1 is used in all example calls
                    string resourceURL = "";
                    string strResult = "";
                    object objResult;
                    Dictionary<string, object> items = new Dictionary<string, object>();

                    // Request for recipient in a group
                    int groupId = -1;
                    if (Session["groupId"] != null) groupId = (int)Session["groupId"];
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/Group/" + groupId + "/Recipients";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);

                    status += $"<p>Request for recipient in a group<br/>{"GET"} {resourceURL} - OK</p>";

                    items = (Dictionary<string, object>)objResult;
                    object[] recipients = (object[])items["Items"];
                    if (recipients.Length > 0) {
                        Dictionary<string, object> recipient = (Dictionary<string, object>)recipients[0];
                        int recipientId = int.Parse(recipient["idRecipient"].ToString());

                        // Pick up a recipient and unsubscribe it
                        resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/Group/" + groupId + "/Unsubscribe/" + recipientId;
                        strResult = mailUp.CallMethod(resourceURL, "DELETE", null, MailUp.ContentType.Json);
                        status += $"<p>Pick up a recipient and unsubscribe it<br/>{"DELETE"} {resourceURL} - OK</p>";
                    }

                    status += "<p>Example methods completed successfully</p>";
                }
            }
            catch (MailUp.MailUpException ex) {
                status += "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode + "<br/>";
            }

            pExampleResultString.InnerHtml = status;
        }
        // EXAMPLE 3 - UPDATE A RECIPIENT DETAIL
        protected void RunExample3_ServerClick(object sender, EventArgs e) {
            string status = "";
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];

            try {
                if (mailUp != null) {
                    // List ID = 1 is used in all example calls
                    string resourceURL = "";
                    string strResult = "";
                    object objResult;
                    Dictionary<string, object> items = new Dictionary<string, object>();

                    // Request for existing subscribed recipients
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Recipients/Subscribed";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    status += $"<p>Request for existing subscribed recipients<br/>{"GET"} {resourceURL} - OK</p>";

                    items = (Dictionary<string, object>)objResult;
                    object[] recipients2 = (object[])items["Items"];
                    if (recipients2.Length > 0) {
                        Dictionary<string, object> recipient = (Dictionary<string, object>)recipients2[0];
                        object[] fields = (object[])recipient["Fields"];
                        if (fields.Length == 0) {
                            object[] arr = new object[1];
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            dict["Id"] = 1;
                            dict["Value"] = "Updated value";
                            dict["Description"] = "";
                            arr[0] = dict;
                            recipient["Fields"] = arr;
                        }
                        else {
                            Dictionary<string, object> dict = (Dictionary<string, object>)fields[0];
                            dict["Id"] = 1;
                            dict["Value"] = "Updated value";
                            dict["Description"] = "";
                        }

                        status += "<p>Modify a recipient from the list - OK</p>";

                        // Update the modified recipient
                        string recipientRequest = new JavaScriptSerializer().Serialize(recipient);
                        resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/Recipient/Detail";
                        strResult = mailUp.CallMethod(resourceURL, "PUT", recipientRequest, MailUp.ContentType.Json);
                        status += $"<p>Update the modified recipient<br/>{"PUT"} {resourceURL} - OK</p>";
                    }

                    status += "<p>Example methods completed successfully</p>";
                }
            }
            catch (MailUp.MailUpException ex) {
                status += "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode + "<br/>";
            }

            pExampleResultString.InnerHtml = status;
        }
        // EXAMPLE 4 - CREATE A MESSAGE FROM TEMPLATE
        protected void RunExample4_ServerClick(object sender, EventArgs e) {
            string status = "";
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];

            try {
                if (mailUp != null) {
                    // List ID = 1 is used in all example calls
                    string resourceURL = "";
                    string strResult = "";
                    object objResult;
                    Dictionary<string, object> items = new Dictionary<string, object>();

                    // Get the available template list
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Templates";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    Dictionary<string, object> template = (Dictionary<string, object>)objResult;
                    Dictionary<string, object> dictionaryItem;
                    int templateId = 0;
                    var arrItems = (object[])template["Items"];
                    if (arrItems.Length > 0) {
                        dictionaryItem = (Dictionary<string, object>)arrItems[0];
                        templateId = (int)dictionaryItem["Id"];
                        status += $"<p>Get the available template list<br/>{"GET"} {resourceURL} - OK</p>";
                    }
                    else {
                        status += $"<p>Could not find any template to create a new message from<br/>{"GET"} {resourceURL} - FAIL</p>";
                    }
                    // Create the new message
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Email/Template/" + templateId;
                    strResult = mailUp.CallMethod(resourceURL, "POST", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    items = (Dictionary<string, object>)objResult;
                    int emailId = int.Parse(items["idMessage"].ToString());
                    status += $"<p>Create the new message<br/>{"POST"} {resourceURL} - OK</p>";

                    // Request for messages list
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Emails";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    status += $"<p>Request for messages list<br/>{"GET"} {resourceURL} - OK</p>";
                    status += "<p>Example methods completed successfully</p>";
                }
            }
            catch (MailUp.MailUpException ex) {
                status += "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode + "<br/>";
            }

            pExampleResultString.InnerHtml = status;
        }
        // EXAMPLE 5 - CREATE A MESSAGE WITH IMAGES AND ATTACHMENTS
        protected void RunExample5_ServerClick(object sender, EventArgs e) {
            string status = "";
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];

            try {
                if (mailUp != null) {
                    // List ID = 1 is used in all example calls
                    string resourceURL = "";
                    string strResult = "";
                    object objResult;
                    Dictionary<string, object> items = new Dictionary<string, object>();

                    // Upload an image
                    // Image bytes can be obtained from file, database or any other source
                    WebClient wc = new WebClient();
                    byte[] imageBytes = wc.DownloadData("https://www.google.it/images/srpr/logo11w.png");
                    string image = System.Convert.ToBase64String(imageBytes);
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Images";
                    string imageRequest = "{\"Base64Data\":\"" + image + "\",\"Name\":\"Avatar\"}";
                    strResult = mailUp.CallMethod(resourceURL, "POST", imageRequest, MailUp.ContentType.Json);
                    status += $"<p>Upload an image<br/>{"PUT"} {resourceURL} - OK</p>";

                    // Get the images available
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/Images";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    string imgSrc = "";
                    object[] srcs = (object[])objResult;
                    if (srcs.Length > 0) imgSrc = srcs[0].ToString();

                    status += $"<p>Get the images available<br/>{"GET"} {resourceURL} - OK</p>";

                    // Create and save "hello" message
                    string message = "&lt;html&gt;&lt;body&gt;&lt;p&gt;Hello&lt;/p&gt;&lt;img src=\\\"" + imgSrc + "\\\"/&gt;&lt;/body&gt;&lt;/html&gt;";
                    message = "<html><body><p>Hello</p><img src=\"" + imgSrc + "\" /></body></html>";
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Email";

                    EmailMessageItemDto dto = new EmailMessageItemDto();
                    dto.Subject = "Test Message c#";
                    dto.IdList = 1;
                    dto.Content = message;
                    dto.Embed = true;
                    dto.IsConfirmation = true;
                    dto.Fields = new List<EmailDynamicFieldDto>();
                    dto.Notes = "Some notes";
                    dto.Tags = new List<EmailTagDto>();
                    dto.TrackingInfo = new EmailTrackingInfoDto() {
                        CustomParams = "",
                        Enabled = true,
                        Protocols = new List<string>() { "http" }
                    };

                    JavaScriptSerializer js = new JavaScriptSerializer();
                    string emailRequest = js.Serialize(dto);
                    strResult = mailUp.CallMethod(resourceURL, "POST", emailRequest, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    items = (Dictionary<string, object>)objResult;
                    Dictionary<string, object> template = (Dictionary<string, object>)objResult;
                    var emailId = template["idMessage"];
                    Session["emailId"] = emailId;

                    status += $"<p>Create and save \"hello\" message<br/>{"POST"} {resourceURL} - OK</p>";

                    // Add an attachment
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Email/" + emailId + "/Attachment/1";
                    string attachment = "QmFzZSA2NCBTdHJlYW0="; // Base64 String
                    string attachmentRequest = "{\"Base64Data\":\"" + attachment + "\",\"Name\":\"TestFile.txt\",\"Slot\":1,\"idList\":1,\"idMessage\":" + emailId + "}";
                    strResult = mailUp.CallMethod(resourceURL, "POST", attachmentRequest, MailUp.ContentType.Json);

                    status += $"<p>Add an attachment<br/>{"POST"} {resourceURL} - OK</p>";

                    // Retrieve message details
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Email/" + emailId;
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    status += $"<p>Retrieve message details<br/>{"GET"} {resourceURL} - OK</p>";
                    status += "<p>Example methods completed successfully</p>";
                }
            }
            catch (MailUp.MailUpException ex) {
                status += "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode + "<br/>";
            }

            pExampleResultString.InnerHtml = status;
        }
        // EXAMPLE 6 - TAG A MESSAGE
        protected void RunExample6_ServerClick(object sender, EventArgs e) {
            string status = "";
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];

            try {
                if (mailUp != null) {
                    // List ID = 1 is used in all example calls
                    string resourceURL = "";
                    string strResult = "";
                    object objResult;
                    Dictionary<string, object> items = new Dictionary<string, object>();

                    // Create a new tag
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Tag";
                    strResult = mailUp.CallMethod(resourceURL, "POST", "\"test tag\"", MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    object[] tags;
                    Dictionary<string, object> tag = (Dictionary<string, object>)objResult;
                    int tagId = int.Parse(tag["Id"].ToString());
                    status += $"<p>Create a new tag<br/>{"POST"} {resourceURL} - OK</p>";

                    // Pick up a message and retrieve detailed informations
                    int emailId = -1;
                    if (Session["emailId"] != null) emailId = (int)Session["emailId"];
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Email/" + emailId;
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    status += $"<p>Pick up a message and retrieve detailed informations<br/>{"GET"} {resourceURL} - OK</p>";

                    // Add the tag to the message details and save
                    Dictionary<string, object> objEmail = (Dictionary<string, object>)objResult;
                    tags = (object[])objEmail["Tags"];
                    List<object> al = new List<object>(tags);
                    Dictionary<string, object> tagItem = new Dictionary<string, object>();
                    tagItem["Id"] = tagId;
                    tagItem["Enabled"] = true;
                    tagItem["Name"] = "test tag";
                    al.Add(tagItem);
                    objEmail["Tags"] = al.ToArray();

                    string emailUpdateRequest = new JavaScriptSerializer().Serialize(objEmail);
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Email/" + emailId;
                    strResult = mailUp.CallMethod(resourceURL, "PUT", emailUpdateRequest, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    status += $"<p>Add the tag to the message details and save<br/>{"PUT"} {resourceURL} - OK</p>";
                    status += "<p>Example methods completed successfully</p>";
                }
            }
            catch (MailUp.MailUpException ex) {
                status += "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode + "<br/>";
            }

            pExampleResultString.InnerHtml = status;
        }
        // EXAMPLE 7 - SEND A MESSAGE
        protected void RunExample7_ServerClick(object sender, EventArgs e) {
            CallMyMethod();
            return;
            string status = "";
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];

            try {
                if (mailUp != null) {
                    // List ID = 1 is used in all example calls
                    string resourceURL = "";
                    string strResult = "";
                    object objResult;
                    Dictionary<string, object> items = new Dictionary<string, object>();

                    //Get the list of the existing messages
                   resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Emails";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    objResult = new JavaScriptSerializer().DeserializeObject(strResult);
                    items = (Dictionary<string, object>)objResult;
                    object[] emails = (object[])items["Items"];
                    Dictionary<string, object> email = (Dictionary<string, object>)emails[0];
                    int emailId = int.Parse(email["idMessage"].ToString());
                    Session["emailId"] = emailId;
                    status += $"<p>Get the list of the existing messages<br/>{"GET"} {resourceURL} - OK</p>";

                    //Send email to all recipients in the list
                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Email/" + emailId + "/Send";
                    strResult = mailUp.CallMethod(resourceURL, "POST", null, MailUp.ContentType.Json);
                    status += $"<p>Send email to all recipients in the list<br/>{"POST"} {resourceURL} - OK</p>";
                    status += "<p>Example methods completed successfully</p>";

                }
            }
            catch (MailUp.MailUpException ex) {
                status += "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode + "<br/>";
            }

            pExampleResultString.InnerHtml = status;
        }

        protected void CallMyMethod() {
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];
            try {
                if (mailUp != null) {
                    string resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/List/1/Recipient";
                    string recipientRequest = "{\"Email\":\"m.angioletti@vargroup.it\",\"Fields\":[{\"Description\":\"String description\",\"Id\":1,\"Value\":\"Mario\"}]," +
                       "\"MobileNumber\":\"\",\"MobilePrefix\":\"\",\"Name\":\"Mariotti\"}";
                    string strResult = mailUp.CallMethod(resourceURL, "POST", recipientRequest, MailUp.ContentType.Json);
                    pResultString.InnerText = txtPath.Text + " returned: " + strResult;

                    resourceURL = "" + mailUp.ConsoleEndpoint + "/Console/Email/Send";
                    recipientRequest = "{\"Email\":\"m.angioletti@vargroup.it\",\"idMessage\":2}";
                    strResult = mailUp.CallMethod(resourceURL, "POST", recipientRequest, MailUp.ContentType.Json);
                    pResultString.InnerText = txtPath.Text + " returned: " + strResult;
                }
            }
            catch (MailUp.MailUpException ex) {
                pResultString.InnerText = "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode;
            }
        }
        // EXAMPLE 8 - DISPLAY STATISTICS FOR A MESSAGE SENT AT EXAMPLE 7
        protected void RunExample8_ServerClick(object sender, EventArgs e) {
            string status = "";
            MailUp.MailUpClient mailUp = (MailUp.MailUpClient)Session["MailUpClient"];

            try {
                if (mailUp != null) {
                    // List ID = 1 is used in all example calls
                    string resourceURL = "";
                    string strResult = "";
                    int emailId = 2;
                    if (Session["emailId"] != null) emailId = (int)Session["emailId"];
                    //resourceURL = "" + mailUp.MailstatisticsEndpoint + "/Message/" + emailId + "/Count/Views?pageSize=5&pageNum=0&filterby=Email.Contains(%27ruscigno%27)";
                    resourceURL = "" + mailUp.MailstatisticsEndpoint + "/Message/" + emailId + "/List/Views?pageSize=5&pageNum=0&filterby=Email.Contains(%27ruscigno%27)";
                    strResult = mailUp.CallMethod(resourceURL, "GET", null, MailUp.ContentType.Json);
                    pResultString.InnerText = strResult;
                    status += $"<p>Request (to MailStatisticsService.svc) for paged message views list for the previously sent message<br/>{"GET"} {resourceURL} - OK</p>";
                    status += "<p>Example methods completed successfully</p>";
                }
            }
            catch (MailUp.MailUpException ex) {
                status += "Exception: " + ex.Message + " with HTTP Status code: " + ex.StatusCode + "<br/>";
            }

            pExampleResultString.InnerHtml = status;
        }
    }
}