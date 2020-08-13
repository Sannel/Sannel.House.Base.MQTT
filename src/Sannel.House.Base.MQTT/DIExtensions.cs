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
	public static class DIExtensions
	{
		/// <summary>
		/// Adds the MQTT service both Subscribe and Publish
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="server">The server.</param>
		/// <param name="defaultTopic">The default topic.</param>
		/// <param name="port">The port.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// services
		/// or
		/// server
		/// </exception>
		public static IServiceCollection AddMqttService(this IServiceCollection services,
			string server,
			string defaultTopic,
			int? port = null)
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
				var options = new MqttClientOptionsBuilder()
					.WithTcpServer(defaultTopic, port)
					.Build();

				return new MqttService(defaultTopic, options, i.GetService<ILogger<MqttService>>());
			});

			services.AddSingleton<IMqttClientPublishService>(i => i.GetService<MqttService>());
			services.AddSingleton<IMqttClientSubscribeService>(i => i.GetService<MqttService>());

			services.AddSingleton<IHostedService>(i => i.GetService<MqttService>());

			return services;
		}
		/// <summary>
		/// Adds the MQTT subscribe service.
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="server">The server.</param>
		/// <param name="port">The port.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// services
		/// or
		/// server
		/// </exception>
		public static IServiceCollection AddMqttSubscribeService(this IServiceCollection services,
			string server,
			int? port = null)
		{
			if(services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if(string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullException(nameof(server));
			}

			services.AddMqttService(server, "unknown", port);

			return services;
		}

		/// <summary>
		/// Adds the MQTT publish service and registers it as a HostService and sets defaultTopic
		/// </summary>
		/// <param name="services">The services.</param>
		/// <param name="server">The server.</param>
		/// <param name="defaultTopic">The default topic.</param>
		/// <param name="port">The port.</param>
		/// <returns></returns>
		/// <exception cref="System.ArgumentNullException">
		/// services
		/// or
		/// server
		/// </exception>
		public static IServiceCollection AddMqttPublishService(this IServiceCollection services, 
			string server,
			string defaultTopic,
			int? port=null)
		{
			if(services is null)
			{
				throw new ArgumentNullException(nameof(services));
			}

			if(string.IsNullOrWhiteSpace(server))
			{
				throw new ArgumentNullException(nameof(server));
			}

			services.AddMqttService(server, defaultTopic, port);

			return services;
		}
	}
}
