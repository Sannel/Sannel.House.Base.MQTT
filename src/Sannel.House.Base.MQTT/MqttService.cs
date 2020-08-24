/* Copyright 2020-2020 Sannel Software, L.L.C.
   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at
      http://www.apache.org/licenses/LICENSE-2.0
   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.*/

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Receiving;
using MQTTnet.Client.Options;
using Sannel.House.Base.MQTT.Interfaces;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Collections.Generic;

namespace Sannel.House.Base.MQTT
{
	public class MqttService : IHostedService,
								IMqttClientConnectedHandler,
								IMqttClientDisconnectedHandler,
								IMqttApplicationMessageReceivedHandler,
								IMqttClientPublishService,
								IMqttClientSubscribeService
	{
		protected readonly ILogger Logger;
		protected readonly IMqttClient MqttClient;
		protected readonly IMqttClientOptions Options;
		protected readonly string DefaultTopic;
		protected readonly ConcurrentDictionary<string, Action<string, string>> Subscriptions = new ConcurrentDictionary<string, Action<string, string>>();
		protected readonly ConcurrentQueue<(string topic, Action<string, string> action)> subscriberQueue = new ConcurrentQueue<(string, Action<string, string>)>();
		protected readonly IServiceProvider services;
		protected readonly SemaphoreSlim locker = new SemaphoreSlim(1);
		private bool shouldReconnect = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="MqttService"/> class.
		/// </summary>
		/// <param name="defaultTopic">The default topic.</param>
		/// <param name="options">The options.</param>
		/// <param name="logger">The logger.</param>
		/// <exception cref="System.ArgumentNullException">
		/// logger
		/// or
		/// options
		/// </exception>
		public MqttService(string defaultTopic, IMqttClientOptions options, IServiceProvider services, ILogger<MqttService> logger)
		{
			this.Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			this.Options = options ?? throw new ArgumentNullException(nameof(options));
			this.services = services ?? throw new ArgumentNullException(nameof(services));
			this.DefaultTopic = defaultTopic;
			MqttClient = new MqttFactory().CreateMqttClient();

			MqttClient.ConnectedHandler = this;
			MqttClient.DisconnectedHandler = this;
			MqttClient.ApplicationMessageReceivedHandler = this;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MqttService"/> class.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="defaultTopic">The default topic.</param>
		/// <param name="options">The options.</param>
		/// <param name="logger">The logger.</param>
		protected MqttService(IMqttClient client, 
			string defaultTopic, 
			IMqttClientOptions options, 
			IServiceProvider services,
			ILogger<MqttService> logger) : this(defaultTopic, options, services, logger)
		{
			this.MqttClient = client;
		}

		/// <summary>
		/// Handles the application message received asynchronous.
		/// </summary>
		/// <param name="eventArgs">The <see cref="MqttApplicationMessageReceivedEventArgs"/> instance containing the event data.</param>
		/// <returns></returns>
		public Task HandleApplicationMessageReceivedAsync(MqttApplicationMessageReceivedEventArgs eventArgs)
			=> Task.Run(() =>
			{
				var topic = eventArgs.ApplicationMessage.Topic;
				if (Subscriptions.TryGetValue(topic, out var callback))
				{
					var payload = Encoding.UTF8.GetString(eventArgs.ApplicationMessage.Payload);
					callback(topic, payload);
				}
			});

		/// <summary>
		/// Handles the connected asynchronous.
		/// </summary>
		/// <param name="eventArgs">The <see cref="MqttClientConnectedEventArgs"/> instance containing the event data.</param>
		/// <returns></returns>
		public Task HandleConnectedAsync(MqttClientConnectedEventArgs eventArgs)
		{
			Logger.LogDebug("Connected ResultCode {0}", eventArgs.AuthenticateResult.ResultCode);
			return Task.CompletedTask;

		}

		/// <summary>
		/// Handles the disconnected asynchronous.
		/// </summary>
		/// <param name="eventArgs">The <see cref="MqttClientDisconnectedEventArgs"/> instance containing the event data.</param>
		public async Task HandleDisconnectedAsync(MqttClientDisconnectedEventArgs eventArgs)
		{
			Logger.LogError("Disconnected ResultCode {0}", eventArgs.ReasonCode);

			if (shouldReconnect)
			{
				await locker.WaitAsync();
				try
				{
					await Task.Delay(TimeSpan.FromSeconds(30));
					Logger.LogInformation("Attempting reconnect");
					await MqttClient.ReconnectAsync();
					Logger.LogInformation("Is Connected {0}", MqttClient.IsConnected);
				}
				finally
				{
					locker.Release();
				}
			}


		}

		/// <summary>
		/// Publish the payload to the default topic
		/// A fire and forget publish where you don't care when its sent
		/// </summary>
		/// <param name="payload">The payload.</param>
		public void Publish(object payload)
			=> Publish(DefaultTopic, payload);

		/// <summary>
		/// Publish the payload to the passed topic
		/// A fire and forget publish where you don't care when its sent
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="payload">The payload.</param>
		public async void Publish(string topic, object payload)
			=> await PublishAsync(topic, payload);

		/// <summary>
		/// Publishes the payload to the default topic
		/// </summary>
		/// <param name="payload">The payload.</param>
		/// <returns></returns>
		public Task PublishAsync(object payload)
			=> PublishAsync(DefaultTopic, payload);

		/// <summary>
		/// Publishes the payload to the passed topic
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="payload">The payload.</param>
		public async Task PublishAsync(string topic, object payload)
		{
			var message = JsonSerializer.Serialize(payload);

			if (Logger.IsEnabled(LogLevel.Debug))
			{
				Logger.LogDebug("MQTT Publish topic {0} payload {1}", topic, message);
			}

			var result = await MqttClient.PublishAsync(topic, message);

			if (Logger.IsEnabled(LogLevel.Debug))
			{
				Logger.LogDebug("MQTT Publish Status {0} topic {1} payload {2}", result.ReasonCode, topic, message);
			}
		}

		/// <summary>
		/// Triggered when the application host is ready to start the service.
		/// </summary>
		/// <param name="cancellationToken">Indicates that the start process has been aborted.</param>
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await locker.WaitAsync(cancellationToken);

			try
			{
				await MqttClient.ConnectAsync(Options);

				if (!MqttClient.IsConnected)
				{
					await MqttClient.ReconnectAsync();
				}

				var subscribers = services.GetServices<IMqttTopicSubscriber>();

				foreach(var sub in subscribers)
				{
					if (sub != null)
					{
						Subscriptions[sub.Topic] = sub.Message;
						await MqttClient.SubscribeAsync(sub.Topic);
					}
				}

				while (!subscriberQueue.IsEmpty)
				{
					if(subscriberQueue.TryDequeue(out var sub))
					{
						Subscriptions[sub.topic] = sub.action;
						await MqttClient.SubscribeAsync(sub.topic);
					}
				}
			}
			finally
			{
				locker.Release();
			}
		}

		/// <summary>
		/// Triggered when the application host is performing a graceful shutdown.
		/// </summary>
		/// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			shouldReconnect = false;
			await MqttClient.DisconnectAsync(new MqttClientDisconnectOptions()
			{
				ReasonCode = MqttClientDisconnectReason.NormalDisconnection,
				ReasonString = "NormalDisconnection"
			});

		}

		/// <summary>
		/// Subscribes to the specified topic with the callback
		/// </summary>
		/// <remarks>
		/// If the same topic is subscribed to again the first call back will be overwritten 
		/// </remarks>
		/// <param name="topic">The topic.</param>
		/// <param name="callback">The callback.</param>
		public async void Subscribe(string topic, Action<string, string> callback)
		{
			await locker.WaitAsync();
			try
			{
				if(MqttClient.IsConnected)
				{
					Subscriptions[topic] = callback;
					await MqttClient.SubscribeAsync(topic);
				}
				else
				{
					subscriberQueue.Enqueue((topic, callback));
				}
			}
			finally
			{
				locker.Release();
			}
		}
	}
}
