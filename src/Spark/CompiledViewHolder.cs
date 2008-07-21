﻿/*
   Copyright 2008 Louis DeJardin - http://whereslou.com

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using System;
using System.Collections.Generic;
using Spark.Compiler;
using Spark.Parser;
using Spark;

namespace Spark
{
    public class CompiledViewHolder
    {
        static private CompiledViewHolder _current;

        readonly Dictionary<Key, Entry> _cache = new Dictionary<Key, Entry>();

        public static CompiledViewHolder Current
        {
            get
            {
                if (_current == null)
                    _current = new CompiledViewHolder();
                return _current;
            }
            set { _current = value; }
        }

        public Entry Lookup(Key key)
        {
            Entry entry;

            lock (_cache)
            {
                if (!_cache.TryGetValue(key, out entry))
                    return null;
            }

            return entry.Loader.IsCurrent() ? entry : null;
        }

        public void Store(Entry entry)
        {
            lock (_cache)
            {
                _cache[entry.Key] = entry;
            }
        }

        public class Key
        {
            public SparkViewDescriptor Descriptor { get; set; }

            public override int GetHashCode()
            {
                return (Descriptor.ControllerName ?? "").ToLowerInvariant().GetHashCode() ^
                       (Descriptor.ViewName ?? "").ToLowerInvariant().GetHashCode() ^
                       (Descriptor.MasterName ?? "").ToLowerInvariant().GetHashCode();
            }

            public override bool Equals(object obj)
            {
                var that = obj as Key;
                if (that == null || GetType() != that.GetType())
                    return false;
                return string.Equals(Descriptor.ControllerName, that.Descriptor.ControllerName, StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(Descriptor.ViewName, that.Descriptor.ViewName, StringComparison.InvariantCultureIgnoreCase) &&
                       string.Equals(Descriptor.MasterName, that.Descriptor.MasterName, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public class Entry : ISparkViewEntry
        {
            public Key Key { get; set; }
            public ViewLoader Loader { get; set; }
            public ViewCompiler Compiler { get; set; }

            public SparkViewDescriptor Descriptor
            {
                get { return Key.Descriptor; }
            }

            string ISparkViewEntry.SourceCode
            {
                get { return Compiler.SourceCode; }
            }

            ISparkView ISparkViewEntry.CreateInstance()
            {
                return Compiler.CreateInstance();
            }
        }
    }
}

