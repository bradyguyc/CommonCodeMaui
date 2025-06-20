using CommunityToolkit.Mvvm.ComponentModel;

using Microsoft.Maui.Storage;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonCode.Helpers;

namespace CommonCode.Models
{
    public class ErrorDetails
    {
        public string ErrorCode;
        public string ErrorTitle;
        public string WhatThisMeans;
        public string WhatYouCanDo;
        public string HelpLink;
        public string Type;
    }

    public static class ErrorDictionary
    {
       

        static public List<ErrorDetails> Errors { get; set; }

        public static async Task<bool> LoadErrorsFromFile()
        {
            try
            {
                string contents = await FileHelpers.ReadTextFile("ErrorDetailsList.json");
                Errors = JsonConvert.DeserializeObject<List<ErrorDetails>>(contents);
                Errors = Errors.Where(e => !string.IsNullOrWhiteSpace(e.ErrorCode)).ToList();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error : {ex.Message} {ex}");
                throw new Exception($"Could not load error descriptions. {ex}");
            }
        }

        public static ErrorDetails GetErrorDetails(string errorCode)
        {
          
            ErrorDetails error = null;
            if (string.IsNullOrWhiteSpace(errorCode))
            {
                ErrorDetails errorDetail = new ErrorDetails
                {
                    ErrorCode = "mnb-999",
                    ErrorTitle = "Error code not defined: " + errorCode,
                    WhatThisMeans = "The error code is not in the list of know error codes. More than likely this is a developer problem and will be resolved with an upcoming release",
                    WhatYouCanDo = "If you have opted in to share analytics and errors with the developer data related to this situation will be provided to the developer so that they can provide a fix with a future release",
                    Type = "Error",
                    HelpLink = "http://helpsite.com/error1"
                };

            }
            if (Errors != null )
            {
                 error = Errors.FirstOrDefault(e => e.ErrorCode == errorCode.Trim());
            }
            

            if (error == null)
            {
                ErrorDetails errorDetail = new ErrorDetails
                {
                    ErrorCode = "mnb-999",
                    ErrorTitle = "Error code not defined: " + errorCode,
                    WhatThisMeans = "The error code is not in the list of know error codes. More than likely this is a developer problem and will be resolved with an upcoming release",
                    WhatYouCanDo = "If you have opted in to share analytics and errors with the developer data related to this situation will be provided to the developer so that they can provide a fix with a future release",
                    Type="Error",
                    HelpLink = "http://helpsite.com/error1"
                };
                return errorDetail;
            }
            return error;
        }
    }

  
}
