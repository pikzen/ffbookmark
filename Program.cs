using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Runtime.Serialization.Json;

namespace ffbookmark
{
    class Program
    {
        

        static void Serialize<T>(T data, string path)
        {
            DataContractJsonSerializer jss = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                jss.WriteObject(ms, data);
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                using (StreamWriter sw = new StreamWriter(path, false))
                {
                    sw.Write(sr.ReadToEnd());
                }
            }
        }
        [STAThread]
        static void Main(string[] args)
        {

            if (args.Length > 0)
            {
                ParseSharliInput(args[0]);
            }
            else
            {
                Console.WriteLine("No input");
            }
        }

        private static bool HasATagChild(HtmlNode node, string tagName)
        {
            if (String.Equals(node.ChildNodes[0].Name, tagName, StringComparison.InvariantCultureIgnoreCase))
                return true;

            return false;
        }

        private static void ParseSharliInput(string input)
        {
            // Setting up the base nodes : root, personal bar, tags, unfiltered, base
            Node baseNode = new Node(null,
                                     "", 
                                     1, 
                                     null, 
                                     DateTime.Now.Ticks, 
                                     DateTime.Now.Ticks, 
                                     null, 
                                     Node.TypeMozPlaceContainer, 
                                     "placesRoot", 
                                     new List<Node>());
            baseNode.Children.Add(new Node(null, 
                                           "Menu des marques-pages", 
                                           2,
                                           1,
                                           DateTime.Now.Ticks, 
                                           DateTime.Now.Ticks, 
                                           null, 
                                           Node.TypeMozPlaceContainer, 
                                           "bookmarksMenuFolder", 
                                           null));
            baseNode.Children.Add(new Node(1,
                                           "Barre Personnelle", 
                                           3, 
                                           1, 
                                           DateTime.Now.Ticks, 
                                           DateTime.Now.Ticks, 
                                           null, 
                                           Node.TypeMozPlaceContainer, 
                                           "toolbarFolder", 
                                           null));
            baseNode.Children.Add(new Node(2,
                                           "Étiquettes",
                                           4,
                                           1,
                                           DateTime.Now.Ticks,
                                           DateTime.Now.Ticks,
                                           null,
                                           Node.TypeMozPlaceContainer,
                                           "tagsFolder",
                                           new List<Node>()));
            baseNode.Children.Add(new Node(3,
                                           "Marque-pages non classés",
                                           5,
                                           1,
                                           DateTime.Now.Ticks,
                                           DateTime.Now.Ticks,
                                           null,
                                           Node.TypeMozPlaceContainer,
                                           "unfiledBookmarksFolder",
                                           new List<Node>()));

            // Getting the HTML in a better state, loading it into a HTMLDocument
            HtmlDocument html = new HtmlDocument();
            FileInfo filePath = new FileInfo(input);

            try
            {
                using (System.IO.StreamReader sr = new System.IO.StreamReader(filePath.FullName))
                {

                    string strHtml = sr.ReadToEnd();
                    strHtml = strHtml.Replace("</A>", "</A></DT>");
                    // Parsing HTML with a Regex.
                    // Might summon Zalgo.
                    // This finds the malformed <DD> tags (not closed).

                    var regex = new System.Text.RegularExpressions.Regex(@"(?<!\>)\n<DT>");
                    strHtml = regex.Replace(strHtml, delegate(System.Text.RegularExpressions.Match m)
                    {
                        return "</DD>" + m.Value;
                    });

                    // Fixing html entities
                    strHtml = HtmlEntity.DeEntitize(strHtml);
                    html.LoadHtml(strHtml);

                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Failure : File does not exist : " + input);
                return;
            }
            

            Console.WriteLine(html.DocumentNode.ChildNodes.Count(node => String.Equals(node.Name, "DT", StringComparison.InvariantCultureIgnoreCase)) + " links found.");


            HtmlNodeCollection nodes = html.DocumentNode.ChildNodes;
            
            /*  Foreach node (tag) in the HTML :
             *   * Look up its name
             *   * If it is a <DT> tag, we found a link
             *   * Look up its child nodes for an <A> tag
             *   * Look up if it has an URL starting with '?', it is a Sharli link, and we ignore it.
             *   * Set the Name as what is linked in the <A> tag
             *   * Look up if it is followed by a <DD> tag, its description
             */
            for (int i = 0; i < nodes.Count; i++)
            {
                if (String.Equals(nodes[i].Name, "DT", StringComparison.InvariantCultureIgnoreCase) && i+2 < nodes.Count)
                {

                    Node annotation = null;

                    // Look up if the next tag is a <DD> tag. If it is, it's this link's description
                    // i+1 would point to a newline tag. (why would you even do that HAP ?
                    if (String.Equals(nodes[i+2].Name, "dd", StringComparison.InvariantCultureIgnoreCase))
                    {
                        annotation = new Node("bookmarkProperties/description", 
                                                    0,
                                                    4,
                                                    null,
                                                    3,
                                                    nodes[i+2].InnerText);
                    }

                    // Shaarli URLs (pointing to itself) causes the bookmark import to fail.
                    // We simply make them point to about:blank
                    string url = nodes[i].ChildNodes[0].Attributes["HREF"].Value;
                    if (url[0] == '?')
                    {
                        url = "about:blank";
                    }
                    baseNode.Children[3].Children.Add(new Node(null,
                                                                   nodes[i].ChildNodes[0].InnerText,
                                                                   i,
                                                                   5,
                                                                   DateTime.Now.Ticks,
                                                                   DateTime.Now.Ticks,
                                                                   "text/x-moz-place",
                                                                   url,
                                                                   annotation));

                    // If this tag has one children which is an <A> tag, it's a link. We get its target.
                    if (HasATagChild(nodes[i], "a"))
                    {
                        HtmlNode currentNode = nodes[i].ChildNodes[0];

                        #region Getting TAGS
                        // If this <A> tag has any "TAGS" attribute, they've been defined by the user, and we're getting them
                        if (currentNode.Attributes.Any(a => String.Equals(a.Name, "TAGS", StringComparison.InvariantCultureIgnoreCase)))
                        {
                            string[] tags = currentNode.Attributes["TAGS"].Value.Split(',');
                            foreach (string tag in tags)
                            {
                                // If there is already one children of the tags which has this tagname, we simply add one children to it
                                if (baseNode.Children[2].Children.Any(c => c.Title == tag))
                                {
                                    Node targetNode = baseNode.Children[2]
                                            .Children
                                            .First(c => c.Title == tag);
                                    targetNode.Children.Add(new Node(null,
                                                                      "",
                                                                      targetNode.ID + targetNode.Children.Count,
                                                                      targetNode.ID,
                                                                      DateTime.Now.Ticks,
                                                                      DateTime.Now.Ticks,
                                                                      "text/x-moz-place",
                                                                      currentNode.Attributes["HREF"].Value,
                                                                      annotation));
                                }
                                else
                                {
                                    baseNode.Children[2].Children.Add(new Node(null,
                                                                           tag,
                                                                           i,
                                                                           4,
                                                                           DateTime.Now.Ticks,
                                                                           DateTime.Now.Ticks,
                                                                           null,
                                                                           Node.TypeMozPlaceContainer,
                                                                           "",
                                                                           new List<Node>()));
                                    Node targetNode = baseNode.Children[2]
                                       .Children
                                       .First(c => c.Title.ToLower() == tag.ToLower());

                                    targetNode.Children.Add(new Node(null,
                                                                       "",
                                                                       targetNode.ID + targetNode.Children.Count,
                                                                       targetNode.ID,
                                                                       DateTime.Now.Ticks,
                                                                       DateTime.Now.Ticks,
                                                                       "text/x-moz-place",
                                                                       currentNode.Attributes["HREF"].Value,
                                                                       annotation));

                                }
                            }
                        }
                        #endregion
                    }
                    

                }
            }
            Serialize<Node>(baseNode, input + ".export.json");
            Console.WriteLine("Sucess ! output written to " + input + ".export.json.");
        }
        
    }
}
