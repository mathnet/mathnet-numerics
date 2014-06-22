namespace Geometry
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    internal static class XmlExt
    {
        public static void WriteValueToReadonlyField<TClass, TProperty>(
            TClass item,
            TProperty value,
            Expression<Func<TProperty>> fieldExpression)
        {
            string name = ((MemberExpression) fieldExpression.Body).Member.Name;
            GetAllFields(item.GetType())
                .Single(x => x.Name == name)
                .SetValue(item, value);
        }

        public static IEnumerable<FieldInfo> GetAllFields(Type t)
        {
            if (t == null)
                return Enumerable.Empty<FieldInfo>();
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static |
                                       BindingFlags.Public | BindingFlags.NonPublic;
            return t.GetFields(bindingAttr)
                    .Concat(GetAllFields(t.BaseType));
        }

        internal static void WriteElement(this XmlWriter writer, string name, IXmlSerializable value)
        {
            writer.WriteStartElement(name);
            value.WriteXml(writer);
            writer.WriteEndElement();
        }

        internal static void WriteElement(this XmlWriter writer, string name, double value)
        {
            writer.WriteStartElement(name);
            writer.WriteValue(value);
            writer.WriteEndElement();
        }

        internal static void WriteElement(this XmlWriter writer, string name, double value, string format)
        {
            writer.WriteElementString(name, value.ToString(format, CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Reads attribute if it exists
        /// 
        /// </summary>
        /// <param name="e"/><param name="localName"/>
        /// <returns/>
        public static string ReadAttributeOrDefault(this XElement e, string localName)
        {
            XAttribute xattribute = e.Attribute(localName);
            if (xattribute != null)
                return xattribute.Value;
            else
                return null;
        }

        public static XElement SingleElement(this XElement e, string localName)
        {
            return e.Elements()
                    .Single(x => x.Name.LocalName == localName);
        }

        public static XElement SingleElementOrDefault(this XElement e, string localName)
        {
            return e.Elements()
                    .SingleOrDefault(x => x.Name.LocalName == localName);
        }

        public static double AsDouble(this XElement e, bool throwIfNull = true, double valueIfNull = 0)
        {
            if (!throwIfNull && e == null)
                return valueIfNull;
            return XmlConvert.ToDouble(e.Value);
        }

        public static double AsDouble(this XAttribute e, bool throwIfNull = true)
        {
            if (!throwIfNull && e == null)
                return 0;
            return XmlConvert.ToDouble(e.Value);
        }

        public static T AsEnum<T>(this XElement e) where T : struct, IConvertible
        {
            return (T) Enum.Parse(typeof (T), e.Value);
        }

        public static T AsEnum<T>(this XAttribute e) where T : struct, IConvertible
        {
            return (T) Enum.Parse(typeof (T), e.Value);
        }

        public static IEnumerable<XElement> ElementsNamed(this XElement e, string localName)
        {
            return e.Elements()
                    .Where(x => x.Name.LocalName == localName);
        }

        public static XAttribute SingleAttribute(this XElement e, string localName)
        {
            return e.Attributes()
                    .Single(x => x.Name.LocalName == localName);
        }

        public static XmlReader SingleElementReader(this XElement e, string localName)
        {
            return e.SingleElement(localName)
                    .CreateReader();
        }

        public static string ReadAttributeOrElement(this XElement e, string localName)
        {
            XAttribute xattribute = e.Attributes()
                                     .SingleOrDefault(x => x.Name.LocalName == localName);
            if (xattribute != null)
                return xattribute.Value;
            XElement xelement = e.Elements()
                                 .SingleOrDefault(x => x.Name.LocalName == localName);
            if (xelement != null)
                return xelement.Value;
            else
                throw new XmlException(String.Format("Attribute or element {0} not found", localName));
        }

        public static T ReadAttributeOrElementEnum<T>(this XElement e, string localName) where T : struct, IConvertible
        {
            return (T) Enum.Parse(typeof (T), e.ReadAttributeOrElement(localName));
        }

        public static string ReadAttributeOrElementOrDefault(this XElement e, string localName)
        {
            XAttribute xattribute = e.Attributes()
                                     .SingleOrDefault(x => x.Name.LocalName == localName);
            if (xattribute != null)
                return xattribute.Value;
            XElement xelement = e.Elements()
                                 .SingleOrDefault(x => x.Name.LocalName == localName);
            if (xelement != null)
                return xelement.Value;
            else
                return null;
        }

        public static XmlWriter WriteAttribute<T>(this XmlWriter writer, string name, T value)
        {
            writer.WriteStartAttribute(name);
            writer.WriteValue(value);
            writer.WriteEndAttribute();
            return writer;
        }

        internal static void SetReadonlyField<TItem, TValue>(
            ref TItem self,
            Expression<Func<TItem, TValue>> func,
            TValue value) where TItem : struct
        {
            var fieldInfo = self.GetType()
                                .GetField(((MemberExpression) func.Body).Member.Name);
            object boxed = self;
            fieldInfo.SetValue(boxed, value);
            self = (TItem) boxed;
        }

        internal static void SetReadonlyField<TItem, TValue>(
            this TItem self,
            Expression<Func<TItem, TValue>> func,
            TValue value) where TItem : class
        {
            var fieldInfo = self.GetType()
                                .GetField(((MemberExpression) func.Body).Member.Name);
            fieldInfo.SetValue(self, value);
        }
    }
}