namespace nyzoNode.Alignment.Core;

public class ByteBuffer {
	private readonly MemoryStream _memoryStream;
	private readonly BinaryWriter _binaryWriter;
	private readonly BinaryReader _binaryReader;

	public ByteBuffer() {
		_memoryStream = new MemoryStream();
		_binaryWriter = new BinaryWriter(_memoryStream);
		_binaryReader = new BinaryReader(_memoryStream);
	}

	// Akin to wrap
	public ByteBuffer(byte[] buffer) {
		_memoryStream = new MemoryStream(buffer);
		_binaryWriter = new BinaryWriter(_memoryStream);
		_binaryReader = new BinaryReader(_memoryStream);
	}

	// Important to test this, just as everything else - todo
	public bool IsEqualToContentInByteBuffer(ByteBuffer byteBuffer) => byteBuffer.GetBytes().ToString() == this.GetBytes().ToString();

	public void PutInt(int value) {
		_binaryWriter.Write(value);
	}

	public int GetInt() {
		return _binaryReader.ReadInt32();
	}

	public void PutString(string value) {
		_binaryWriter.Write(value);
	}

	public string GetString() {
		return _binaryReader.ReadString();
	}

	public void PutBoolean(bool value) {
		_binaryWriter.Write(value);
	}

	public bool GetBoolean() {
		return _binaryReader.ReadBoolean();
	}

	public byte[] GetBytes() {
		return _memoryStream.ToArray();
	}

	public void Clear() {
		_memoryStream.SetLength(0);
		_memoryStream.Position = 0;
	}

	public void Dispose() {
		_binaryWriter.Dispose();
		_binaryReader.Dispose();
		_memoryStream.Dispose();
	}
}
