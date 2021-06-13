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
	/// interface to publish messages to the underlying message provider
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public interface IMessagePublisher : IDisposable
	{
		/// <summary>
		/// Publishes the specified message object to the underlying message provider
		/// </summary>
		/// <param name="messageObject">The message object.</param>
		void Publish(object messageObject);

		/// <summary>
		/// Publishes the specified message object to the underlying message provider
		/// </summary>
		/// <param name="messageObject">The message object.</param>
		/// <returns></returns>
		Task PublishAsync(object messageObject);
	}
}
