using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace ffbookmark
{
    [DataContract]
    public class Node
    {
        [DataMember(Name = "title", IsRequired=true, Order=1)]
        public string Title;

        [DataMember(Name = "id", IsRequired = false)]
        public int ID;

        [DataMember(Name = "parent", IsRequired = false, EmitDefaultValue=false)]
        public int? Parent;

        [DataMember(Name = "annos", IsRequired = false, EmitDefaultValue=false)]
        public List<Node> Annotations;

        [DataMember(Name = "children", IsRequired = false,EmitDefaultValue=false)]
        public List<Node> Children;

        [DataMember(Name = "index", IsRequired = false, EmitDefaultValue = false)]
        public int? Index;

        [DataMember(Name = "dateAdded", Order = 4, IsRequired = false, EmitDefaultValue = false)]
        public long DateAdded;

        [DataMember(Name = "lastModified", Order = 5, IsRequired = false, EmitDefaultValue = false)]
        public long DateModified;

        [DataMember(Name = "root", Order = 7, IsRequired = false, EmitDefaultValue = false)]
        public string Root;

        [DataMember(Name = "type", IsRequired = false, EmitDefaultValue = false)]
        string Type;

        [DataMember(Name = "uri", IsRequired = false, EmitDefaultValue = false)]
        string URI;

        [DataMember(Name = "name", IsRequired = false, EmitDefaultValue = false)]
        string Name;

        [DataMember(Name = "flags", IsRequired = false, EmitDefaultValue = false)]
        int Flags;

        [DataMember(Name = "expires", IsRequired = false, EmitDefaultValue = false)]
        int Expires;

        [DataMember(Name = "mimeType", IsRequired = false, EmitDefaultValue = false)]
        string MimeType;

        [DataMember(Name = "value", IsRequired = false, EmitDefaultValue = false)]
        string Value;

        public static String TypeMozPlaceContainer = "text/x-moz-place-container";

        /// <summary>
        /// Classic node constructor
        /// </summary>
        /// <param name="idx">Index</param>
        /// <param name="title">Title</param>
        /// <param name="id">Identifier</param>
        /// <param name="parent">Parent node</param>
        /// <param name="da">Creation date</param>
        /// <param name="dm">Last modified</param>
        /// <param name="anno">Annotations</param>
        /// <param name="type">Type</param>
        /// <param name="root">Root element ?</param>
        /// <param name="children">Children</param>
        public Node(int? idx, string title, int id, int? parent, long da, long dm, Node anno, string type, string root, List<Node> children)
            :base()
        {
            Index = idx;
            Title = title;
            ID = id;
            Parent = parent;
            DateAdded = da;
            DateModified = dm;
            if (anno != null)
            {
                Annotations = new List<Node>();
                Annotations.Add(anno);
            }
            Type = type;
            Root = root;
            Children = children;
        }

        /// <summary>
        /// Build this Node as a Bookmark
        /// </summary>
        /// <param name="index">Index</param>
        /// <param name="title">Title</param>
        /// <param name="id">Identifier</param>
        /// <param name="parent">Parent node</param>
        /// <param name="da">Creation date</param>
        /// <param name="dm">Last modified</param>
        /// <param name="type">Type</param>
        /// <param name="uri">URI</param>
        /// <param name="anno">Annotations</param>
        public Node(int? index,string title, int id, int parent, long da, long dm, string type, string uri, Node anno)
            :base()
        {
            Children = new List<Node>();
            Index = index;
            Title = title;
            ID = id;
            Parent = parent;
            DateAdded = da;
            DateModified = dm;
            Type = type;
            if (anno != null)
            {
                Annotations = new List<Node>();
                Annotations.Add(anno);
            }
            URI = uri;
        }

        /// <summary>
        /// Build this node as an annotation
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="flags">Flags</param>
        /// <param name="expires">Expires on</param>
        /// <param name="mime">Mime Type</param>
        /// <param name="type">Type</param>
        /// <param name="desc">Description</param>
        public Node(string name, int flags, int expires, string mime, int type, string desc)
        {
            Name = name;
            Flags = flags;
            Expires = expires;
            MimeType = mime;
            Type = type.ToString();
            Value = desc;
        }

    }
}
