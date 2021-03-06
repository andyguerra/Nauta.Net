﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Nauta.Web
{

    public class LoginResponse
    {
        public string Session { get; set; }
        public string TimeParams { get; set; }
        public bool AlreadyConnected { get; set; }
        public bool NoMoney { get; set; }
        public bool BadUsername { get; set; }
        public bool BadPassword { get; set; }
    }

    class ResponseParser
    {

        public Dictionary<string, string> ParseHomeResponse(string[] response)
        {
            string pattern1 = "<input type=\"hidden\" name=\"([A-Za-z0-9./]*)\" id=\"[A-Za-z0-9./]*\" value=\"([A-Za-z0-9./]*)\"";
            string pattern2 = "<input type='hidden' name='CSRFHW' value='([A-Za-z0-9]*)'";

            var dict = new Dictionary<string, string>();
            foreach (string resp in response)
            {
                var line = resp.Trim();
                var match = Regex.Match(line, pattern1, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine("{0} --> {1}", match.Groups[1], match.Groups[2]);
                    dict.Add(match.Groups[1].ToString(), match.Groups[2].ToString());
                }

                match = Regex.Match(line, pattern2, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    if (!dict.ContainsKey("CSRFHW"))
                    {
                        Console.WriteLine("CSRFHW --> {0}", match.Groups[1]);
                        dict.Add("CSRFHW", match.Groups[1].ToString());
                    }
                }
            }
            return dict;
        }

        public LoginResponse ParseLoginResponse(string[] response)
        {
            string pattern1 = "var urlParam = \"([A-Za-z0-9=_&]*)\"";
            string pattern2 = "g_httpRequest.open\\(\"post\", \"/EtecsaQueryServlet\\?([A-Za-z0-9=_&@.]*)\", true\\);";
            string pattern3 = "alert\\(\"Su tarjeta no tiene saldo disponible";
            string pattern4 = "alert\\(\"El usuario ya está conectado.\"\\);";
            string pattern5 = "alert\\(\"Entre el nombre de usuario y contraseña correctos";
            string pattern6 = "alert\\(\"No se pudo autorizar al usuario";

            string session = null;
            string timeParams = "";
            bool alreadyConnected = false;
            bool noMoney = false;
            bool badPassword = false;
            bool badUsername = false;

            foreach (string resp in response)
            {
                var line = resp.Trim();
                Console.WriteLine(line);

                var match = Regex.Match(line, pattern1, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine("{0}", match.Groups[1]);
                    session = match.Groups[1].ToString();
                }

                match = Regex.Match(line, pattern2, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine("{0}", match.Groups[1]);
                    timeParams = match.Groups[1].ToString();
                }

                match = Regex.Match(line, pattern3, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine("{0}", match.Groups[0]);
                    noMoney = true;
                }

                match = Regex.Match(line, pattern4, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine("{0}", match.Groups[0]);
                    alreadyConnected = true;
                }

                match = Regex.Match(line, pattern5, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine("{0}", match.Groups[0]);
                    badPassword = true;
                }

                match = Regex.Match(line, pattern6, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    Console.WriteLine("{0}", match.Groups[0]);
                    badUsername = true;
                }
            }

            if (session == null && !alreadyConnected && !noMoney && !badPassword && !badUsername)
                return null;

            return new LoginResponse
            {
                Session = session,
                TimeParams = timeParams,
                AlreadyConnected = alreadyConnected,
                NoMoney = noMoney,
                BadPassword = badPassword,
                BadUsername = badUsername
            };
        }

        public bool ParseLogoutResponse(string[] response)
        {
            if (response == null || response.Length == 0)
                return false;

            var line = response[0].Trim();
            Console.WriteLine(line);
            return line.Equals("logoutcallback('SUCCESS');");
        }

    }

}
