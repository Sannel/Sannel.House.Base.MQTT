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

using System.Threading.Tasks;

namespace Sannel.House.Base.MQTT.Interfaces
{
	/// <summary>
	/// Represents an interface for publishing to mqtt
	/// </summary>
	public interface IMqttClientPublishService
	{
		/// <summary>
		/// Publish the payload to the default topic
		/// A fire and forget publish where you don't care when its sent
		/// </summary>
		/// <param name="payload">The payload.</param>
		/// <returns></returns>
		void Publish(object payload);
		/// <summary>
		/// Publish the payload to the passed topic
		/// A fire and forget publish where you don't care when its sent
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="payload">The payload.</param>
		/// <returns></returns>
		void Publish(string topic, object payload);

		/// <summary>
		/// Publishes the payload to the default topic
		/// </summary>
		/// <param name="payload">The payload.</param>
		/// <returns></returns>
		Task PublishAsync(object payload);

		/// <summary>
		/// Publishes the payload to the passed topic
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="payload">The payload.</param>
		/// <returns></returns>
		Task PublishAsync(string topic, object payload);

	}
}
