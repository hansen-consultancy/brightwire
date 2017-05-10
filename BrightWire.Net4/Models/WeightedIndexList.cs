﻿using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace BrightWire.Models
{
    /// <summary>
    /// A sparse array of weighted indices
    /// </summary>
    [ProtoContract]
    public class WeightedIndexList
    {
        /// <summary>
        /// A weighted index
        /// </summary>
        [ProtoContract]
        public class WeightedIndex
        {
            /// <summary>
            /// Index
            /// </summary>
            [ProtoMember(1)]
            public uint Index { get; set; }

            /// <summary>
            /// Index weight
            /// </summary>
            [ProtoMember(2)]
            public float Weight { get; set; }
        }

        /// <summary>
        /// The list of indices
        /// </summary>
        [ProtoMember(1)]
        public WeightedIndex[] IndexList { get; set; }

        /// <summary>
        /// The number of items in the list
        /// </summary>
        public int Count { get { return IndexList?.Length ?? 0; } }

        /// <summary>
        /// ToString override
        /// </summary>
        public override string ToString()
        {
            return $"{Count} indices";
        }

        /// <summary>
        /// Writes the data to an XML writer
        /// </summary>
        /// <param name="name">The name to give the data</param>
        /// <param name="writer">The writer to write to</param>
        public void WriteTo(string name, XmlWriter writer)
        {
            writer.WriteStartElement(name ?? "weighted-index-list");

            if (IndexList != null) {
                writer.WriteValue(String.Join("|", IndexList
                    .OrderBy(d => d.Index)
                    .Select(c => $"{c.Index}:{c.Weight}")
                ));
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the data to a binary writer
        /// </summary>
        /// <param name="writer"></param>
        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Count);
            if (IndexList != null) {
                foreach (var item in IndexList) {
                    writer.Write(item.Index);
                    writer.Write(item.Weight);
                }
            }
        }

        /// <summary>
        /// Creates a weighted index list from a binary reader
        /// </summary>
        /// <param name="reader">The binary reader</param>
        public static WeightedIndexList ReadFrom(BinaryReader reader)
        {
            var len = reader.ReadInt32();
            var ret = new WeightedIndex[len];

            for (var i = 0; i < len; i++) {
                var category = new WeightedIndex();
                category.Index = reader.ReadUInt32();
                category.Weight = reader.ReadSingle();
                ret[i] = category;
            }

            return new WeightedIndexList {
                IndexList = ret
            };
        }
    }
}