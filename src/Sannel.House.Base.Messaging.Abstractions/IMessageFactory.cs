/* Copyright 2021-2021 Sannel Software, L.L.C.
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
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Base.Messaging
{
	/// <summary>
	/// Represents a Factory for creating Publishers and Subscribers for messaging 
	/// </summary>
	public interface IMessageFactory : IDisposable
	{
		/// <summary>
		/// Creates the publisher for the topic.
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <returns></returns>
		IMessagePublisher CreatePublisherForTopic(string topic);

		/// <summary>
		/// Creates the publisher for topic asynchronous.
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <returns></returns>
		Task<IMessagePublisher> CreatePublisherForTopicAsync(string topic);

		/// <summary>
		/// Subscribes to topic.
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="callback">The callback.</param>
		void SubscribeToTopic(string topic, Action<string, string> callback);

		/// <summary>
		/// Subscribes to topic.
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="callbackAsync">The callback asynchronous.</param>
		void SubscribeToTopic(string topic, Func<string, string, Task> callbackAsync);
	}
}
