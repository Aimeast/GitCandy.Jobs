using GitCandy.Log;
using GitCandy.Schedules;
using Microsoft.Web.Administration;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web.Hosting;

namespace GitCandy.Jobs
{
    // Auto access self to avoid web app recycling.
    // In order to get the binding, please set the Application Pool Identities as LocalSystem.
    // See, http://www.iis.net/learn/manage/configuring-security/application-pool-identities
    public class KeepAliveJob : IJob
    {
        private Uri _uri;

        public KeepAliveJob()
        {
            Logger.Info("Init KeepAliveJob");
            _uri = null;

            var sm = new ServerManager();
            var id = HostingEnvironment.ApplicationHost.GetSiteID();
            var site = sm.Sites.FirstOrDefault(s => s.Id.ToString() == id);
            if (site == null)
            {
                return;
            }

            var binding = site.Bindings.FirstOrDefault(s =>
                string.Equals(s.Protocol, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
                ?? site.Bindings.FirstOrDefault();
            if (binding == null)
            {
                return;
            }
            Logger.Info(binding.ToString());

            var host = binding.Host == "*" || binding.Host == ""
                ? (binding.EndPoint.Address == IPAddress.Any || binding.EndPoint.Address == IPAddress.IPv6Any
                    ? binding.EndPoint.Address
                    : binding.EndPoint.Address.AddressFamily == AddressFamily.InterNetworkV6
                        ? IPAddress.IPv6Loopback
                        : IPAddress.Loopback).ToString()
                : binding.Host;

            IPAddress ip;
            if (IPAddress.TryParse(host, out ip) && ip.AddressFamily == AddressFamily.InterNetworkV6)
                host = "[" + host + "]";

            _uri = new Uri(string.Format("{0}://{1}:{2}/",
                binding.Protocol,
                host,
                binding.EndPoint.Port));

            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                var req = sender as HttpWebRequest;
                return req != null && string.Equals(req.Host, _uri.Host, StringComparison.OrdinalIgnoreCase);
            };

            Logger.Info("KeepAliveJob, Uri is {0}", _uri);
        }

        public void Execute(JobContext jobContext)
        {
            if (_uri == null)
                return;

            // Normally, the certificate trusted by host
            var wc = new WebClient();
            wc.DownloadData(_uri);
        }

        public TimeSpan GetNextInterval(JobContext jobContext)
        {
            return _uri == null ? TimeSpan.MaxValue : TimeSpan.FromMinutes(3);
        }

        public TimeSpan Due
        {
            get { return TimeSpan.FromSeconds(1); }
        }
    }
}
