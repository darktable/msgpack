namespace MsgPack {
	public interface IMessagePackable
	{
	    void ToMsgPack(Packer packer);
	    void FromMsgPack(Unpacker unpacker);
	}
}