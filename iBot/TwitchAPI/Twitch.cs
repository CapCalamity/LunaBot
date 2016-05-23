﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using IBot.Core;
using IBot.Core.Settings;
using IBot.TwitchAPI.Models;
using Newtonsoft.Json;
using NLog;
using RestSharp;

namespace IBot.TwitchAPI
{
    public static class Twitch
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private const string TwitchApiBase = "https://api.twitch.tv/kraken";
        private const string ChannelSubscriptionFormat = "/channels/{0}/subscriptions";
        private const string ChannelFollowerFormat = "/channels/{0}/follows";

        /// <summary>
        /// client_id    = your client ID
        /// redirect_uri = your registered redirect URI
        /// scope        = space separated list of scopes
        /// state        = your provided unique token
        /// </summary>
        private const string UserAuth = "/oauth2/authorize?response_type=token&client_id={0}&redirect_uri={1}&scope={2}";

        private static readonly RestClient Client;

        static Twitch()
        {
            Client = new RestClient(TwitchApiBase);
        }

        public static string GetAuthenticationUrl(string clientId, string redirectUrl, Scope scope) 
            => string.Format(UserAuth, clientId, redirectUrl, scope.Concat("+"));

        private static void CallTwitch(string url,
                                       Action<IRestResponse> successAction,
                                       Action<Error, IRestResponse> failureAction,
                                       Method method = Method.GET,
                                       Dictionary<HttpStatusCode, Action<IRestResponse>> responseHandlers = null,
                                       HttpStatusCode expectedStatusCode = HttpStatusCode.OK)
        {
            var request = new RestRequest(url, method);
            request.AddHeader("accept", "application/vnd.twitchtv.v3+json");
            request.AddHeader("Authorization", SettingsManager.GetSettings<ConnectionSettings>().OwnerTwitchApiKey.Replace("oauth:", "OAuth "));

            var response = Client.Execute(request);

            if (response.StatusCode == expectedStatusCode)
            {
                try
                {
                    successAction?.Invoke(response);
                }
                catch (Exception e)
                {
                    _logger.Warn(e);
                }
            }
            else
            {
                Error error = null;
                try
                {
                    error = JsonConvert.DeserializeObject<Error>(response.Content);
                }
                catch (JsonException e)
                {
                    _logger.Debug(e);
                }

                try
                {
                    failureAction?.Invoke(error, response);
                }
                catch (Exception e)
                {
                    _logger.Warn(e);
                }
            }

            foreach (var handler in responseHandlers)
            {
                if (handler.Key == response.StatusCode)
                {
                    try
                    {
                        handler.Value?.Invoke(response);
                    }
                    catch (Exception e)
                    {
                        _logger.Trace(e);
                    }
                }
            }
        }

        public static IEnumerable<string> GetChannelSubscribers(string channel) => GetChannelSubscribers(channel, null);

        private static IEnumerable<string> GetChannelSubscribers(string channel, string pageUrl)
        {
            var retVal = new List<string>();

            // ReSharper disable once ArgumentsStyleOther
            CallTwitch(url: string.IsNullOrWhiteSpace(pageUrl)
                                ? string.Format(ChannelSubscriptionFormat, channel)
                                : pageUrl,
                       successAction: (response) =>
                       {
                           var responseDefinition = new
                           {
                               _total = 0,
                               _links = new Links(),
                               subscriptions = new List<User>(),
                           };
                           try
                           {
                               var content = JsonConvert.DeserializeAnonymousType(response.Content, responseDefinition);
                               retVal.AddRange(content.subscriptions.Select(s => s.Name));

                               if (content.subscriptions.Count > 0)
                                   retVal.AddRange(GetChannelSubscribers(channel, content._links.Next));
                           }
                           catch (JsonException e)
                           {
                               _logger.Debug(e);
                               retVal = new List<string>();
                           }
                       },
                       failureAction: (e, response) =>
                       {
                           _logger.Warn("channel {0}, status: {1}, type: {2}, message: {3} - unexpected result", channel, e.Status, e.Type, e.Message);
                           retVal = new List<string>();
                       },
                       responseHandlers: new Dictionary<HttpStatusCode, Action<IRestResponse>>()
                       {
                           { (HttpStatusCode) 422, response => { _logger.Trace("channel {0} has no subscribers - expected result", channel); } },
                           { HttpStatusCode.Forbidden, response => { _logger.Trace("not authorized to view subscribers of channel {0}", channel); } },
                       });

            return retVal;
        }

        public static IEnumerable<string> GetChannelFollowers(string channel) => GetChannelFollowers(channel, null);

        private static IEnumerable<string> GetChannelFollowers(string channel, string pageUrl)
        {
            var retVal = new List<string>();

            // ReSharper disable once ArgumentsStyleOther
            CallTwitch(url: string.IsNullOrWhiteSpace(pageUrl)
                                ? string.Format(ChannelFollowerFormat, channel)
                                : pageUrl,
                       successAction: (response) =>
                       {
                           var responseDefinition = new
                           {
                               _total = 0,
                               _links = new Links(),
                               _cursor = "",
                               follows = new List<Follow>(),
                           };
                           try
                           {
                               var content = JsonConvert.DeserializeAnonymousType(response.Content, responseDefinition);
                               retVal.AddRange(content.follows.Select(f => f.User.Name));

                               if (content.follows.Count > 0)
                                   retVal.AddRange(GetChannelFollowers(channel, content._links.Next));
                           }
                           catch (JsonException e)
                           {
                               _logger.Debug(e);
                               retVal = new List<string>();
                           }
                       },
                       failureAction: (e, response) =>
                       {
                           _logger.Warn("channel {0}, status: {1}, type: {2}, message: {3} - unexpected result", channel, e.Status, e.Type, e.Message);
                           retVal = new List<string>();
                       },
                       responseHandlers: new Dictionary<HttpStatusCode, Action<IRestResponse>>()
                       {
                           { (HttpStatusCode) 422, response => _logger.Trace("channel {0} has no followers - expected result", channel) },
                           { HttpStatusCode.Forbidden, response => _logger.Trace("not authorized to view followers of channel {0}", channel) },
                       });

            return retVal;
        }
    }
}
