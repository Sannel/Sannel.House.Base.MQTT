using System;
using System.Collections.Generic;
using System.Text;

namespace Sannel.House.Base.MQTT.Interfaces
{
	public interface IMqttTopicSubscriber
	{
		/// <summary>
		/// Gets the topic to subscribe to
		/// </summary>
		/// <value>
		/// The topic.
		/// </value>
		string Topic { get; }

		/// <summary>
		/// Message from a topic
		/// </summary>
		/// <param name="topic">The topic.</param>
		/// <param name="message">The message.</param>
		void Message(string topic, string message);
	}
}
