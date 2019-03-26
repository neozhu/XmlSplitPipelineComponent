# Custom Pipeline Component - Easy Method to Split Large XML File Using LINQ to XML
You have a large well formed XML file which you wish to split into smaller manageable files. Each output file is also a well formed XML file. This approach uses Skip and Take LINQ extension methods to intuitively slice and dice the source XML into smaller parts.
# Code
``` 
public void Disassemble(Microsoft.BizTalk.Component.Interop.IPipelineContext pc, Microsoft.BizTalk.Message.Interop.IBaseMessage inmsg)
        {
            // 
            // TODO: implement message retrieval logic
             IBaseMessagePart Body = inmsg.BodyPart;
             if (Body != null)
             {
                 Stream originalStream = Body.GetOriginalDataStream();
                 if (originalStream != null)
                 {
                     var xml = XElement.Load(originalStream);
                     var rootElement = xml.Name;
                     // Child elements from source file to split by.
                     var childNodes = xml.Descendants(this.DescendantElement);

                     // This is the total number of elements to be sliced up into 
                     // separate files.
                     int cnt = childNodes.Count();

                     var skip = 0;
                     var take = this.BatchSize;
                     var fileno = 0;

                     // Split elements into chunks and save to disk.
                     while (skip < cnt)
                     {
                         // Extract portion of the xml elements.
                         var c1 = childNodes
                                     .Skip(skip)
                                     .Take(take);

                         // Setup number of elements to skip on next iteration.
                         skip += take;
                         // File sequence no for split file.
                         fileno += 1;
                         // Filename for split file.
                         // Create a partial xml document.
                         XElement frag = new XElement(rootElement, c1);
                         // Save to disk.
                         var newStream = new MemoryStream();
                         frag.Save(newStream);
                         newStream.Position = 0;
                        pc.ResourceTracker.AddResource(newStream);
                         IBaseMessage newmsg = pc.GetMessageFactory().CreateMessage();
                        newmsg.AddPart("Body", pc.GetMessageFactory().CreateMessagePart(), true);
                        newmsg.BodyPart.Data = newStream;
                        newmsg.Context = PipelineUtil.CloneMessageContext(inmsg.Context);
                        //outMsg.Context.Promote("MessageType",  "http://schemas.microsoft.com/BizTalk/2003/system-properties",      "Namespace#Root");
                        var msgtype = (string.IsNullOrEmpty(rootElement.Namespace.NamespaceName)?"" : rootElement.Namespace.NamespaceName +"#") + rootElement.LocalName;
                        newmsg.Context.Write("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", msgtype);
                        newmsg.Context.Promote("MessageType", "http://schemas.microsoft.com/BizTalk/2003/system-properties", msgtype);
                     
                         _msgs.Enqueue(newmsg);

                     }

                 }
             }

          
        }
```
