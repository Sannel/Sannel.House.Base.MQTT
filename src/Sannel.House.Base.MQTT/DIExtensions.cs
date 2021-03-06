using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet.Client.Options;
using Sannel.House.Base.MQTT;
using Sannel.House.Base.MQTT.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// 
	/// </summary>
	public static class DIExtensions
	{
		/// <summary>
		/// Adds the MQTT service both Subscribe and Publish
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="server">The server.</param>
		/// <param name="defaultTopic">The default topic.</param>
		/// <param name="port">The port.</param>
		/// <param name="ssl">if set to <c>true</c> [SSL].</param>
		/// <param name="certSubject">The cert subject.</param>
		/// <param name="certIssuer">The cert issuer.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// services
		/// or
		/// server
		/// </exception>
		/// <exception cref="System.ArgumentNullException">services
		/// or
		/// server</exception>
		public static IServiceCollection AddMqttService(this IServiceCollection services,
			string server,
			string defaultTopic,
			int? port = null,
			bool ssl = false,
			string certSubject = null,
			string certIssuer = null,
			string username = null,
			string password = null)
		{
			if(services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if(string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullException(nameof(server));
			}

			services.AddSingleton<MqttService>(i =>
			{
				var optionBuilder = new MqttClientOptionsBuilder()
					.WithTcpServer(server, port);

				if(!string.IsNullOrWhiteSpace(username)
					&& !string.IsNullOrWhiteSpace(password))
				{
					optionBuilder.WithCredentials(username, password);
				}

				if(ssl)
				{
					optionBuilder.WithTls(p =>
					{
						p.UseTls = true;
						if(!string.IsNullOrWhiteSpace(certSubject)
							&& !string.IsNullOrWhiteSpace(certIssuer))
						{
							p.CertificateValidationHandler = c =>
							{
								return
									string.Equals(certSubject, c.Certificate.Subject, StringComparison.OrdinalIgnoreCase)
									&& string.Equals(certIssuer, c.Certificate.Issuer, StringComparison.OrdinalIgnoreCase);
							};
						}
					});
				}

				var options = optionBuilder
					.Build();

				return new MqttService(defaultTopic, 
					options, 
					i, 
					i.GetService<IConfiguration>(), 
					i.GetService<ILogger<MqttService>>());
			});

			services.AddSingleton<IMqttClientPublishService>(i => i.GetService<MqttService>());
			services.AddSingleton<IMqttClientSubscribeService>(i => i.GetService<MqttService>());

			services.AddSingleton<IHostedService>(i => i.GetService<MqttService>());

			return services;
		}

		/// <summary>
		/// Adds the MQTT service.
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="configuration">The configuration.</param>
		/// <example>
		/// Example Configuration Json. Only Server, and DefaultTopic are Required.
		/// {
		///     "MQTT": {
		///         "Server": "localhost",
		///         "DefaultTopic": "local/house",
		///         "Port": 1883,
		///         "EnableSSL": true,
		///         "CertSubject": "CN=test.com",
		///         "CertIssuer": "CN=Let's Encrypt Authority X3, O=Let's Encrypt, C=US",
		///         "Username": "Hodierne",
		///         "Password": "Fridayweed"
		///     }
		/// }
		/// </example>
		/// <returns></returns>
		public static IServiceCollection AddMqttService(this IServiceCollection services, IConfiguration configuration) 
			=> AddMqttService(services,
				server: configuration["MQTT:Server"],
				defaultTopic: configuration["MQTT:DefaultTopic"],
				port: configuration.GetValue<int?>("MQTT:Port"),
				ssl: configuration.GetValue<bool>("MQTT:EnableSSL"),
				certSubject: configuration["MQTT:CertSubject"],
				certIssuer: configuration["MQTT:CertIssuer"],
				username: configuration["MQTT:Username"],
				password: configuration["MQTT:Password"]);

		/// <summary>
		/// Adds the MQTT subscribe service.
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="server">The server.</param>
		/// <param name="port">The port.</param>
		/// <param name="ssl">if set to <c>true</c> [SSL].</param>
		/// <param name="certSubject">The cert subject.</param>
		/// <param name="certIssuer">The cert issuer.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// services
		/// or
		/// server
		/// </exception>
		/// <exception cref="System.ArgumentNullException">services
		/// or
		/// server</exception>
		public static IServiceCollection AddMqttSubscribeService(this IServiceCollection services,
			string server,
			int? port = null,
			bool ssl = false,
			string certSubject = null,
			string certIssuer = null,
			string username = null,
			string password = null)
		{
			if(services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if(string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullException(nameof(server));
			}

			services.AddMqttService(server, "unknown", port, ssl, certSubject, certIssuer, username, password);

			return services;
		}

		/// <summary>
		/// Adds the MQTT publish service and registers it as a HostService and sets defaultTopic
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="server">The server.</param>
		/// <param name="defaultTopic">The default topic.</param>
		/// <param name="port">The port.</param>
		/// <param name="ssl">if set to <c>true</c> [SSL].</param>
		/// <param name="certSubject">The cert subject.</param>
		/// <param name="certIssuer">The cert issuer.</param>
		/// <param name="username">The username.</param>
		/// <param name="password">The password.</param>
		/// <returns></returns>
		/// <exception cref="ArgumentNullException">
		/// services
		/// or
		/// server
		/// </exception>
		/// <exception cref="System.ArgumentNullException">services
		/// or
		/// server</exception>
		public static IServiceCollection AddMqttPublishService(this IServiceCollection services, 
			string server,
			string defaultTopic,
			int? port = null,
			bool ssl = false,
			string certSubject = null,
			string certIssuer = null,
			string username = null,
			string password = null)
		{
			if(services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if(string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullException(nameof(server));
			}

			services.AddMqttService(server, defaultTopic, port, ssl, certSubject, certIssuer, username, password);

			return services;
		}
	}
}
