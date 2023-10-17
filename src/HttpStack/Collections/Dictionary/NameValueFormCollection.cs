using System.Collections.Specialized;

namespace HttpStack.Collections;

public class NameValueFormCollection : NameValueDictionary, IFormCollection
{
	public NameValueFormCollection(NameValueCollection nameValueCollection, IFormFileCollection files)
		: base(nameValueCollection)
	{
		Files = files;
	}

	public NameValueFormCollection(NameValueCollection nameValueCollection)
		: this(nameValueCollection, EmptyFormFileCollection.Instance)
	{
	}

	public NameValueFormCollection()
	{
		Files = EmptyFormFileCollection.Instance;
	}

	public IFormFileCollection Files { get; private set; }

	public virtual void SetFormFileCollection(IFormFileCollection files)
	{
		Files = files;
	}

	public override void Reset()
	{
		base.Reset();

		if (Files is FormFileCollection collection)
		{
			collection.Reset();
		}
		else
		{
			Files = EmptyFormFileCollection.Instance;
		}
	}
}
