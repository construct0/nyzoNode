namespace nyzoNode.Alignment.Core;

public static class DictionaryExtensions {
	public static TValue? TryGetByteBufferValue<TKey, TValue>(this Dictionary<TKey, TValue> dict, ByteBuffer
		 byteBuffer, TValue? defaultValue=default) where TKey : ByteBuffer {
		foreach(var kvp in dict) {
			if(kvp.Key.IsEqualToContentInByteBuffer(byteBuffer)) {
				return kvp.Value;
			}
		}

		return defaultValue;
	}
}
