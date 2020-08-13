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

using Microsoft.Extensions.Logging;
using Moq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Sannel.House.Base.MQTT.Tests.Access;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sannel.House.Base.MQTT.Tests
{
	public class PublishTest
	{
		[Fact]
		public async Task PublishAsyncDefaultTest()
		{
			var defaultTopic = "publish/defaultTopic";
			string messageJson = null;

			var callback = 0;

			var client = new Moq.Mock<IMqttClient>();
			client.Setup(i => i.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<System.Threading.CancellationToken>()))
				.Callback<MqttApplicationMessage, CancellationToken>((message, token) =>
				{
					callback++;
					Assert.Equal(Encoding.UTF8.GetBytes(messageJson), message.Payload);
					Assert.Equal(defaultTopic, message.Topic);
				});

			var service = new MqttServiceAccess(client.Object, defaultTopic, new MqttClientOptions(),
				(new Mock<ILogger<MqttService>>()).Object);

			var payload = new
			{
				DeviceId = 500,
				DateCreated = DateTimeOffset.Now
			};

			messageJson = JsonSerializer.Serialize(payload);

			await service.PublishAsync(payload);

			Assert.Equal(1, callback);
		}

		[Fact]
		public async Task PublishAsyncTest()
		{
			var defaultTopic = "publish/defaultTopic";
			var topic = "publish1/topic1";
			string messageJson = null;

			var callback = 0;

			var client = new Moq.Mock<IMqttClient>();
			client.Setup(i => i.PublishAsync(It.IsAny<MqttApplicationMessage>(), It.IsAny<System.Threading.CancellationToken>()))
				.Callback<MqttApplicationMessage, CancellationToken>((message, token) =>
				{
					callback++;

					Assert.Equal(topic, message.Topic);

					Assert.Equal(Encoding.UTF8.GetBytes(messageJson), message.Payload);
				});

			var service = new MqttServiceAccess(client.Object, defaultTopic, new MqttClientOptions(),
				(new Mock<ILogger<MqttService>>()).Object);

			object payload = new
			{
				DeviceId = 500,
				DateCreated = DateTimeOffset.Now
			};

			messageJson = JsonSerializer.Serialize(payload);

			await service.PublishAsync(topic, payload);

			Assert.Equal(1, callback);

			payload = new
			{
				SensorId=3,
				SensorData=22
			};

			topic = "publich2/topic2";

			messageJson = JsonSerializer.Serialize(payload);

			await service.PublishAsync(topic, payload);

			Assert.Equal(2, callback);
		}
	}
}
