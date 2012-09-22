using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Security.Cryptography;

public partial class es5_auth : System.Web.UI.Page
{
    static byte[] literal2bytes(string literal)
    {
        if (literal.Length % 2 != 0)
        {
            literal = "0" + literal;
        }
        var r = new byte[literal.Length / 2];



        for (var i = 0; i < literal.Length / 2; i += 1)
        {
            r[i] = Convert.ToByte(literal[i * 2].ToString() + literal[i * 2 + 1].ToString(), 16);
        }

        return r;
    }
    protected void Page_Load(object sender, EventArgs e)
    {
        var code = Request.QueryString["code"];


        //send request to github server to get access token
        HttpWebRequest req = WebRequest.Create("https://github.com/login/oauth/access_token?client_id=TODO:<your own client id>&client_secret=TODO:<your own client secret>&code=" + code) as HttpWebRequest;
        req.Method = "POST";
        HttpWebResponse rsps = req.GetResponse() as HttpWebResponse;
        var str = new StreamReader(rsps.GetResponseStream()).ReadToEnd();
        Match m = Regex.Match(str, "access_token=([^&]+)&token_type=([^&]+)");


        //RSA encrypt access token with public key from browser side
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        RSAParameters publicKey = new RSAParameters();
        publicKey.Modulus = literal2bytes(Request.Cookies["modulus"].Value);
        publicKey.Exponent = literal2bytes(Request.Cookies["exponent"].Value);
        rsa.ImportParameters(publicKey);
        byte[] result = rsa.Encrypt(Encoding.UTF8.GetBytes(m.Groups[1].ToString()), false);
        StringBuilder access_token = new StringBuilder();
        for (var i = 0; i < result.Length; i++)
        {
            access_token.Append(result[i].ToString("x2"));
        }

        //write encrypted access_token back into cookie
        HttpCookie cookie = new HttpCookie("access_token");
        DateTime dt = DateTime.Now;
        TimeSpan ts = new TimeSpan(0, 0, 0, 0);
        cookie.Expires = dt.Add(ts);
        cookie.Value = access_token.ToString();
        Response.AppendCookie(cookie);


        cookie = new HttpCookie("token_type");
        dt = DateTime.Now;
        ts = new TimeSpan(0, 0, 0, 0);
        cookie.Expires = dt.Add(ts);
        cookie.Value = m.Groups[2].ToString();
        Response.AppendCookie(cookie);

        //now jump back, only the browser side could decrypt the access_token from cookie
        Response.Redirect("TODO:<your own address>");
    }
}
