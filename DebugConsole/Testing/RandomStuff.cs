using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
 using BalsamicSolutions.ReadUnreadSiteColumn;

namespace DebugConsole
{
    /// <summary>
    /// this class is used to generate random user information
    /// mostly used for testing databases and UX
    /// </summary>
    public class RandomStuff
    {
        static string[] EMAIL_DOMAINS= new string[]{"gmail.com","hotmail.com","yahoo.com","facebook.com","msn.com"};
                                    
        static readonly Random _RandomNumberGenerator = new Random();

        static IList<string> _GivenNames = null;
        static IList<string> _FemaleSurNames = null;
        static IList<string> _MaleSurNames = null;
        static IList<string> _Sentances = null;

     
        /// <summary>
        /// randomize the order of a list object
        /// </summary>
        /// <typeparam name="TObjectType">type of object in list</typeparam>
        /// <param name="shuffleThis">list to randomize</param>
        public static void RandomizeList<TObjectType>(IList<TObjectType> shuffleThis)
        {
            int listIdx = shuffleThis.Count;
            while (listIdx > 1)
            {
                listIdx--;
                int newPos = _RandomNumberGenerator.Next(listIdx + 1);
                TObjectType value = shuffleThis[newPos];
                shuffleThis[newPos] = shuffleThis[listIdx];
                shuffleThis[listIdx] = value;
            }
        }

        /// <summary>
        /// randomize the order of an array
        /// </summary>
        /// <param name="shuffleThis">array to randomize</param>
        public static void RandomizeArray(object[] shuffleThis)
        {
            int listIdx = shuffleThis.Length;
            while (listIdx > 1)
            {
                listIdx--;
                int newPos = _RandomNumberGenerator.Next(listIdx + 1);
                object value = shuffleThis[newPos];
                shuffleThis[newPos] = shuffleThis[listIdx];
                shuffleThis[listIdx] = value;
            }
        }
 
        /// <summary>
        /// Generate a random phone number in teh same area code
        /// and exchange as the provided phone number
        /// </summary>
        /// <param name="adjacentNumber"></param>
        /// <returns></returns>
        public static string RandomPhoneNumber( string adjacentNumber)
        {
            adjacentNumber = adjacentNumber.Replace("(", string.Empty).Replace(")", string.Empty).Replace("-", string.Empty);
            string returnValue = "(" + adjacentNumber.Substring(0, 3) + ")";
            if (adjacentNumber.Length > 5)
            {
                returnValue += adjacentNumber.Substring(3, 3);
            }
            else
            {
                returnValue += _RandomNumberGenerator.Next(200, 999).ToString();
            }
            returnValue += "-" + _RandomNumberGenerator.Next(1000, 9999).ToString();

            return returnValue;
        }

        /// <summary>
        /// generate a random US formated phone number
        /// </summary>
        /// <returns></returns>
        public static string RandomPhoneNumber()
        {
            string returnValue = "(";
            returnValue += _RandomNumberGenerator.Next(200, 799).ToString();
            returnValue += ")";
            returnValue += _RandomNumberGenerator.Next(200, 999).ToString();
            returnValue += "-" + _RandomNumberGenerator.Next(1000, 9999).ToString();

            return returnValue;
        }

        /// <summary>
        /// generate a random list of users from the provided email domains 
        /// and active directory domains
        /// </summary>
        /// <param name="numOfNames">number of names to generate</param>
        /// <param name="emailDomains">valid email domain suffixes</param>
        /// <param name="adDomains">valid active directory domain names</param>
        /// <returns></returns>
        public static List<NameInfo> RandomNames(int numOfNames, string[] emailDomains, string[] adDomains, int minNameLength = 4)
        {
            if (null == emailDomains || emailDomains.Length == 0)
            {
                emailDomains = EMAIL_DOMAINS;
            }
            if (null == adDomains || adDomains.Length == 0)
            {
                adDomains = new string[] { "@domain.local" };
            }
            for (int mailIdx = 0; mailIdx < emailDomains.Length; mailIdx++)
            {
                if (!emailDomains[mailIdx].StartsWith("@"))
                {
                    emailDomains[mailIdx] = "@" + emailDomains[mailIdx];
                }
            }
            for (int mailIdx = 0; mailIdx < adDomains.Length; mailIdx++)
            {
                if (!adDomains[mailIdx].StartsWith("@"))
                {
                    adDomains[mailIdx] = "@" + adDomains[mailIdx];
                }
            }
            Dictionary<string, NameInfo> uniqueValues = new Dictionary<string, NameInfo>();
            List<NameInfo> returnValue = new List<NameInfo>();
            while (returnValue.Count < numOfNames)
            {
                string firstName = RandomSurName();
                string lastName = RandomGivenName();
                string emailDomain = RandomSelect(emailDomains);
                string adDomain = RandomSelect(adDomains);
                NameInfo addMe = new NameInfo(firstName, lastName, emailDomain, adDomain);
                if (!uniqueValues.ContainsKey(addMe.UserPrincipalName)
                    && !uniqueValues.ContainsKey(addMe.sAMAccountName)
                    && !uniqueValues.ContainsKey(addMe.Email)
                    && addMe.sAMAccountName.Length>=minNameLength)
                {
                    uniqueValues.Add(addMe.UserPrincipalName,addMe);
                    uniqueValues.Add(addMe.Email, addMe);
                    uniqueValues.Add(addMe.sAMAccountName, addMe);
                    returnValue.Add(addMe);
                }
            }
            return returnValue;
        
        }

        static string RandomSelect(string[] arrayOfValues)
        {
            string returnValue = arrayOfValues[0];
            if (arrayOfValues.Length > 1)
            {
                returnValue = arrayOfValues[_RandomNumberGenerator.Next(0,arrayOfValues.Length)];
            }
            return returnValue;
        }

        public static string RandomSentance(int minLength=1,int maxLength =4096)
        {
            return RandomSentance(Sentences, minLength, maxLength);
        }

        public static string RandomSentance(IList<string> candidateText, int minLength = 1, int maxLength = 4096)
        {
            string returnValue=string.Empty;
            while (returnValue.Length < minLength)
            {
                returnValue += candidateText[_RandomNumberGenerator.Next(0, candidateText.Count)];
                for(int idx=0;idx<_RandomNumberGenerator.Next(0,3);idx++)
                {
                    returnValue += candidateText[_RandomNumberGenerator.Next(0, candidateText.Count)];
                }
            }
            return returnValue.TrimTo(maxLength);
        }

        public static string RandomGivenName()
        {
            return GivenNames[_RandomNumberGenerator.Next(0, _GivenNames.Count)];
        }

        public static string RandomSurName()
        {
            if (_RandomNumberGenerator.Next(0, 100) > 60)
            {
                return FemaleSurNames[_RandomNumberGenerator.Next(0, _FemaleSurNames.Count)];
            }
            else
            {
                return MaleSurNames[_RandomNumberGenerator.Next(0, _MaleSurNames.Count)];
            }
        }

        public static IList<string> Sentences
        {
            get
            {
                if (null == _Sentances)
                {
                    _Sentances = LoadText("DebugConsole.Testing.Sentences.txt").AsReadOnly();
                }
                return _Sentances;
            }
        }

        public static IList<string> GivenNames
        {
            get
            {
                if (null == _GivenNames)
                {
                    _GivenNames = LoadNames("DebugConsole.Testing.dist.all.last.txt").AsReadOnly();
                }
                return _GivenNames;
            }
        }

        public static IList<string> SurNames()
        {
            List<string> returnValue = new List<string>();
            returnValue.AddRange(FemaleSurNames);
            returnValue.AddRange(MaleSurNames);
            return returnValue.AsReadOnly();
        }

        public static IList<string> FemaleSurNames
        {
            get
            {
                if (null == _FemaleSurNames)
                {
                    _FemaleSurNames = LoadNames("DebugConsole.Testing.dist.female.first.txt").AsReadOnly();
                }
                return _FemaleSurNames;
            }
        }

        public static IList<string> MaleSurNames
        {
            get
            {
                if (null == _MaleSurNames)
                {
                    _MaleSurNames = LoadNames("DebugConsole.Testing.dist.male.first.txt").AsReadOnly();
                }
                return _MaleSurNames;
            }
        }

        static List<string> LoadNames(string resName)
        {
            List<string> returnValue = new List<string>();
            var thisAssm = Assembly.GetExecutingAssembly();
            Stream ioStream = thisAssm.GetManifestResourceStream(resName);
            using (StreamReader srNames = new StreamReader(ioStream))
            {
                string lineText = srNames.ReadLine();
                while (null != lineText && lineText.Length > 0)
                {
                    string nameText = lineText.Trim();
                    string prettyName = nameText.Substring(0, 1).ToUpper() + nameText.Substring(1).ToLower();
                    returnValue.Add(prettyName);
                    lineText = srNames.ReadLine();
                }
            }
            return returnValue;
        }

        static List<string> LoadText(string resName)
        {
            List<string> returnValue = new List<string>();
            var thisAssm = Assembly.GetExecutingAssembly();
            Stream ioStream = thisAssm.GetManifestResourceStream(resName);
            using (StreamReader srNames = new StreamReader(ioStream))
            {
                string lineText = srNames.ReadLine();
                while (null != lineText && lineText.Length > 0)
                {
                    returnValue.Add(lineText);
                    lineText = srNames.ReadLine();
                }
            }
            return returnValue;
        }
     
    }
}

 