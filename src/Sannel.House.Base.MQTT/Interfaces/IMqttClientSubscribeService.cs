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

using System;

namespace Sannel.House.Base.MQTT.Interfaces
{
	/// <summary>
	/// Represents an interface for subscribing to an mqtt topic
	/// </summary>
	public interface IMqttClientSubscribeService
	{
		/// <summary>
		/// Subscribes the specified topic.
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="callback">The callback.</param>
		void Subscribe(string topic, Action<string, string> callback);
	}
}
