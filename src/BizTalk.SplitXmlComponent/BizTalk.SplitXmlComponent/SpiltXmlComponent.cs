namespace BizTalk.PiplineComponent.Receiver
{
    using System;
    using System.IO;
    using System.Text;
    using System.Drawing;
    using System.Resources;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using System.ComponentModel;
    using Microsoft.BizTalk.Message.Interop;
    using Microsoft.BizTalk.Component.Interop;
    using Microsoft.BizTalk.Component;
    using Microsoft.BizTalk.Messaging;
    using System.Xml.Linq;
    using System.Collections.Generic;
    using System.Linq;
    
    
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [System.Runtime.InteropServices.Guid("cb4c9895-defc-41f9-80a6-1de67b30dda9")]
    [ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
    public class SpiltXmlComponent : Microsoft.BizTalk.Component.Interop.IDisassemblerComponent, IBaseComponent, IPersistPropertyBag, IComponentUI
    {
        
        private System.Resources.ResourceManager resourceManager = new System.Resources.ResourceManager("BizTalk.PiplineComponent.Receiver.SpiltXmlComponent", Assembly.GetExecutingAssembly());
        
        private int _BatchSize;
        
        public int BatchSize
        {
            get
            {
                return _BatchSize;
            }
            set
            {
                _BatchSize = value;
            }
        }
        
        private string _DescendantElement;
        
        public string DescendantElement
        {
            get
            {
                return _DescendantElement;
            }
            set
            {
                _DescendantElement = value;
            }
        }
        
        #region IBaseComponent members
        /// <summary>
        /// Name of the component
        /// </summary>
        [Browsable(false)]
        public string Name
        {
            get
            {
                return resourceManager.GetString("COMPONENTNAME", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// Version of the component
        /// </summary>
        [Browsable(false)]
        public string Version
        {
            get
            {
                return resourceManager.GetString("COMPONENTVERSION", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        
        /// <summary>
        /// Description of the component
        /// </summary>
        [Browsable(false)]
        public string Description
        {
            get
            {
                return resourceManager.GetString("COMPONENTDESCRIPTION", System.Globalization.CultureInfo.InvariantCulture);
            }
        }
        #endregion
        
        #region IPersistPropertyBag members
        /// <summary>
        /// Gets class ID of component for usage from unmanaged code.
        /// </summary>
        /// <param name="classid">
        /// Class ID of the component
        /// </param>
        public void GetClassID(out System.Guid classid)
        {
            classid = new System.Guid("cb4c9895-defc-41f9-80a6-1de67b30dda9");
        }
        
        /// <summary>
        /// not implemented
        /// </summary>
        public void InitNew()
        {
        }
        
        /// <summary>
        /// Loads configuration properties for the component
        /// </summary>
        /// <param name="pb">Configuration property bag</param>
        /// <param name="errlog">Error status</param>
        public virtual void Load(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, int errlog)
        {
            object val = null;
            val = this.ReadPropertyBag(pb, "BatchSize");
            if ((val != null))
            {
                this._BatchSize = ((int)(val));
            }
            val = this.ReadPropertyBag(pb, "DescendantElement");
            if ((val != null))
            {
                this._DescendantElement = ((string)(val));
            }
        }
        
        /// <summary>
        /// Saves the current component configuration into the property bag
        /// </summary>
        /// <param name="pb">Configuration property bag</param>
        /// <param name="fClearDirty">not used</param>
        /// <param name="fSaveAllProperties">not used</param>
        public virtual void Save(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, bool fClearDirty, bool fSaveAllProperties)
        {
            this.WritePropertyBag(pb, "BatchSize", this.BatchSize);
            this.WritePropertyBag(pb, "DescendantElement", this.DescendantElement);
        }
        
        #region utility functionality
        /// <summary>
        /// Reads property value from property bag
        /// </summary>
        /// <param name="pb">Property bag</param>
        /// <param name="propName">Name of property</param>
        /// <returns>Value of the property</returns>
        private object ReadPropertyBag(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, string propName)
        {
            object val = null;
            try
            {
                pb.Read(propName, out val, 0);
            }
            catch (System.ArgumentException )
            {
                return val;
            }
            catch (System.Exception e)
            {
                throw new System.ApplicationException(e.Message);
            }
            return val;
        }
        
        /// <summary>
        /// Writes property values into a property bag.
        /// </summary>
        /// <param name="pb">Property bag.</param>
        /// <param name="propName">Name of property.</param>
        /// <param name="val">Value of property.</param>
        private void WritePropertyBag(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, string propName, object val)
        {
            try
            {
                pb.Write(propName, ref val);
            }
            catch (System.Exception e)
            {
                throw new System.ApplicationException(e.Message);
            }
        }
        #endregion
        #endregion
        
        #region IComponentUI members
        /// <summary>
        /// Component icon to use in BizTalk Editor
        /// </summary>
        [Browsable(false)]
        public IntPtr Icon
        {
            get
            {
                return ((System.Drawing.Bitmap)(this.resourceManager.GetObject("COMPONENTICON", System.Globalization.CultureInfo.InvariantCulture))).GetHicon();
            }
        }
        
        /// <summary>
        /// The Validate method is called by the BizTalk Editor during the build 
        /// of a BizTalk project.
        /// </summary>
        /// <param name="obj">An Object containing the configuration properties.</param>
        /// <returns>The IEnumerator enables the caller to enumerate through a collection of strings containing error messages. These error messages appear as compiler error messages. To report successful property validation, the method should return an empty enumerator.</returns>
        public System.Collections.IEnumerator Validate(object obj)
        {
            // example implementation:
            // ArrayList errorList = new ArrayList();
            // errorList.Add("This is a compiler error");
            // return errorList.GetEnumerator();
            return null;
        }
        #endregion
        
        /// <summary>
        /// this variable will contain any message generated by the Disassemble method
        /// </summary>
        private System.Collections.Queue _msgs = new System.Collections.Queue();
        
        #region IDisassemblerComponent members
        /// <summary>
        /// called by the messaging engine until returned null, after disassemble has been called
        /// </summary>
        /// <param name="pc">the pipeline context</param>
        /// <returns>an IBaseMessage instance representing the message created</returns>
        public Microsoft.BizTalk.Message.Interop.IBaseMessage GetNext(Microsoft.BizTalk.Component.Interop.IPipelineContext pc)
        {
            // get the next message from the Queue and return it
            Microsoft.BizTalk.Message.Interop.IBaseMessage msg = null;
            if ((_msgs.Count > 0))
            {
                msg = ((Microsoft.BizTalk.Message.Interop.IBaseMessage)(_msgs.Dequeue()));
            }
            return msg;
        }
        
        /// <summary>
        /// called by the messaging engine when a new message arrives
        /// </summary>
        /// <param name="pc">the pipeline context</param>
        /// <param name="inmsg">the actual message</param>
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
        #endregion
    }
}
