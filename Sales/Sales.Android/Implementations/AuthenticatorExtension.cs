using System;
using Xamarin.Auth;

public class AuthenticatorExtensions : OAuth2Authenticator
{
    public AuthenticatorExtensions(string clientId, string clientSecret, string scope, Uri authorizeUrl, Uri redirectUrl, Uri accessTokenUrl , GetUsernameAsyncFunc getUsernameAsync = null, bool isUsingNativeUI = false) : base(clientId, clientSecret, scope, authorizeUrl, redirectUrl, accessTokenUrl, getUsernameAsync, isUsingNativeUI)
    {
    }
    protected override void OnPageEncountered(Uri url, System.Collections.Generic.IDictionary<string, string> query, System.Collections.Generic.IDictionary<string, string> fragment)
    {
        // Remove state from dictionaries. 
        // We are ignoring request state forgery status 
        // as we're hitting an ASP.NET service which forwards 
        // to a third-party OAuth service itself
        if (query.ContainsKey("state"))
        {
            query.Remove("state");
        }

        if (fragment.ContainsKey("state"))
        {
            fragment.Remove("state");
        }

        base.OnPageEncountered(url, query, fragment);
    }
}