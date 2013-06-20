﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using IOPath = System.IO.Path;

/* Copyright © 2012 Vadim Baklanov (Ad), distributed under the MIT License
 * When copying, use or create derivative works do not remove or modify this attribution, and this license text.*/

namespace System.IO
{
    /// <summary>This class encapsulates a file system path (not shell path!) and wraps Path, File, Directory static class methods
    /// from System.IO namespace, provides FileInfo DirectoryInfo objects, has additional properties and methods.
    /// See also my ShellPathInfo</summary>
    [XmlRoot("Path")]
	[Serializable]
    [ComVisible(true)]
	public partial class PathInfo : IComparable, IComparable<PathInfo>, ISerializable, IXmlSerializable
	{
        // Constants

        public static char[] PathSeparators = new[] { IOPath.DirectorySeparatorChar, IOPath.AltDirectorySeparatorChar };
        public static char[] InvalidFileNameChars = IOPath.GetInvalidFileNameChars();

        // Basic attributes

        /// <summary>Gets a value indicating whether the path is relative to the Base path.</summary>
        public bool IsRelative
        {
            get { return ((object)_Base != null); }
        }

        bool _Empty = true;
        public bool Empty
        {
            get { return _Empty; }
        }

        public bool Immutable {get;set;}

        PathInfo _Base;
        /// <summary>Base for relative form of path. May be null.</summary>
        public PathInfo Base
        {
            get { return _Base; }
            set
            {
                if (Immutable)
                    throw new InvalidOperationException("This PathInfo object is immutable!");

                _Base = value;
                _FullPathUpperCase = null;

                _name_retrieved     = false;
                _Name               = null;
                file_name_retrieved = false;
                _FileName           = null;
                extension_retrieved = false;
                _Extension          = null;
                parent_retrieved    = false;
                _Parent             = null;
                segments_retrieved  = false;
                _Segments           = null;
                directory_info_retrieved = false;
                _DirectoryInfo      = null;
                file_info_retrieved = false;
                _FileInfo           = null;

                _Empty = ((object)_Path == null || _Path.Length == 0) && ((object)_Base == null || _Base.Empty);
            }
        }

        string _Path;
        /// <summary>Full path or relative to the Base path.</summary>
        public string Path
        {
            get { return _Path; }
            set
            {
                if (Immutable)
                    throw new InvalidOperationException("This PathInfo object is immutable!");

                _Path = value;
                _FullPathUpperCase = null;

                _name_retrieved     = false;
                _Name               = null;
                file_name_retrieved = false;
                _FileName           = null;
                extension_retrieved = false;
                _Extension          = null;
                parent_retrieved    = false;
                _Parent             = null;
                segments_retrieved  = false;
                _Segments           = null;
                directory_info_retrieved = false;
                _DirectoryInfo      = null;
                file_info_retrieved = false;
                _FileInfo           = null;

                _Empty = ((object)_Path == null || _Path.Length == 0) && ((object)_Base == null || _Base.Empty);
            }
        }

        // TODO
        public PathInfo ToRelativePath(PathInfo base_path)
        {
            // Получить относительный путь, относительно указанного базового

            //if ((object)_Base == null)
            //{
            //    // Путь не является относительным

            //    if (FullPathUpperCase.StartsWith(base_path.PathLowerCase))
            //    {
            //        // Да, это относительный путь, конструируем объект относительного пути:
            //        return new PathInfo(Path.Substring(base_path.Path.Length + 1)) { Base = new PathInfo(base_path) };
            //    }
            //}

            return null;
        }

        bool segments_retrieved;
        string[] _Segments;
        public string[] Segments
        {
            get
            {
                if (!segments_retrieved)
                {
                    _Segments = FullPath.Split(PathSeparators);
                    segments_retrieved = true;
                }
                return _Segments;
            }
        }

        string _FullPath;
        /// <summary>Get the full path</summary>
		public string FullPath
        {
            get
            {
                if (_FullPath == null)
                {
                    if ((object)_Base != null)
                    {
                        _FullPath = _Base.FullPath;

                        if (_Path != null)
                            _FullPath += IOPath.DirectorySeparatorChar + _Path;
                    }
                    else
                        _FullPath = _Path;
                }

                return _FullPath;
            }
        }

        /// <summary>Full path to uppercase for case-insensitive comparison and quick search in lists.
        /// The need for this requisite arises from the lack of guarantees from the operating system to return the file path in the same case. Drive char case in equivalent path may be different as result different API requests.
        /// </summary>
        /// 
        string _FullPathUpperCase;
        public string FullPathUpperCase
        {
            get
            {
                if (_FullPathUpperCase == null && (_Path != null || (object)_Base != null))
                    _FullPathUpperCase = FullPath.ToUpperInvariant();

                return _FullPathUpperCase;
            }
        }

		string _FileName;
		bool file_name_retrieved;
        /// <summary>File name and extension</summary>
	    public string FileName
		{
			get
			{
				if (!file_name_retrieved)
				{
					_FileName = IOPath.GetFileName(Path);
					file_name_retrieved = true;
				}

				return _FileName;
			}
            set
            {
                if ((object)_Path == null || _Path.Length == 0)
                    Path = value;
                else
                {
                    var parent_path_string = IOPath.GetDirectoryName(Path);
                    Path = IOPath.Combine(parent_path_string, value);
                }
            }
		}

		string _Name;
		bool _name_retrieved;
        /// <summary>File name without extension</summary>
	    public string Name
		{
			get
			{
				if (!_name_retrieved)
				{
					_Name = IOPath.GetFileNameWithoutExtension(Path);
					_name_retrieved = true;
				}

				return _Name;
			}
		}

		string _Extension;
		bool extension_retrieved;
        /// <summary>File extension</summary>
	    public string Extension
		{
			get
			{
				if (!extension_retrieved)
				{
					_Extension = IOPath.GetExtension(Path);
					extension_retrieved = true;
				}

				return _Extension;
			}
		}

		PathInfo _Parent;
		bool parent_retrieved;
        /// <summary>Get parent path</summary>
	    public virtual PathInfo Parent
		{
            // TODO relative path Parent??? How do it ...?

			get
			{
				if (!parent_retrieved)
				{
					parent_retrieved = true;

					if ((object)_Path == null || _Path.Length == 0)
                    {
						_Parent = null;
                    }
					else
					{
                        // TODO relative path
						_Parent = new PathInfo(IOPath.GetDirectoryName(FullPath)) { Immutable = true };
					}
				}
					
				return _Parent;
			}
		}
        
        FileInfo _FileInfo;
		bool file_info_retrieved;
        public FileInfo FileInfo
		{
            get
            {
                if (!file_info_retrieved)
                {
                    _FileInfo = new FileInfo(FullPath);
                    file_info_retrieved = true;
                }
			    
                return _FileInfo;
            }
		}

        DirectoryInfo _DirectoryInfo;
		bool directory_info_retrieved;
        public DirectoryInfo DirectoryInfo
		{
            get
            {
                if (!directory_info_retrieved)
                {
                    _DirectoryInfo = new DirectoryInfo(FullPath);
                    directory_info_retrieved = true;
                }
			    
                return _DirectoryInfo;
            }
		}

        // From and to string conversion
        
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static implicit operator PathInfo(string path)
        {
            return new PathInfo(path);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
		public static implicit operator string(PathInfo path_info)
		{
            if ((object)path_info == null)
                return null;

            return path_info.FullPath;
		}

        public override string ToString()
		{
            return FullPath;
		}

        // Combine / operator

        /// <summary> / - operator combines PathInfo and string to the new PathInfo</summary>
        public static PathInfo operator /(PathInfo path, string segment)
        {
            return path.Combine(segment);
        }

        // Equality comparers
        
        /// <summary>'Case-insensitive' hash code - hash code from FullPathUpperCase string.</summary>
        public override int GetHashCode()
        {
            var fullupper = FullPathUpperCase;
            return (fullupper == null) ? 0 : fullupper.GetHashCode();
        }

        /// <summary>Invariant ignore case equality of paths comparer. Empty (unassigned) PathInfo equal other empty PathInfo.</summary>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false; // Guidelines for Overloading Equals()

            // compare object references

            if ((object)this == obj)
                return true;

            // compare to PathInfo

            string other_path_string = null;

            var other_string = obj as string;
            if (other_string != null)
            {
                other_path_string = other_string;
            }
            else
            {
                var other = obj as PathInfo;
                if ((object)other != null)
                    other_path_string = other.FullPathUpperCase;              // Later this eliminate internal to upper case transformation in string.Equals
            }

            if (other_path_string == null)
                return true; // Guidelines for Overloading Equals()

            // compare to other path as string

            if (Empty)
                return false;

            return string.Equals(
                FullPathUpperCase,                              // Eliminate internal to upper case transformation
                other_path_string, 
                StringComparison.InvariantCultureIgnoreCase);   // Windows always treats file names and Universal Resource Identifiers as invariant
        }

        /// <summary>Invariant ignore case equality of paths comparer. Empty (unassigned) PathInfo equal other empty PathInfo.</summary>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator ==(PathInfo path1, PathInfo path2)
        {
            if ((object)path1 == null)
                return ((object)path2 == null || path2.Empty);

            if ((object)path1 == (object)path2)
                return true;

            return path1.Equals(path2);
        }

        /// <summary>Invariant ignore case inequality of paths comparer. Empty (unassigned) PathInfo equal other empty PathInfo.</summary>
        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public static bool operator !=(PathInfo path1, PathInfo path2)
        {
            if ((object)path1 == null)
                return ((object)path2 != null && !path2.Empty);

            return !path1.Equals(path2);
        }

        /// <summary>Invariant ignore case of paths comparer.</summary>
        public static bool operator >(PathInfo path1, PathInfo path2)
        {
            if ((object)path1 == null)
                return false;
            
            if ((object)path2 == null)
                return true;

            // Compare path as string is incorrect, compare segments of path separately at each level of the hierarchy is the right way

            var segments1 = path1.Segments;
            var segments2 = path2.Segments;

            // compare common part of of these two paths

            for(int i = 0, c1 = segments1.Length, c2 = segments2.Length; i < c1 && i < c2; i++)
            {
                var comparsion_result = string.Compare(segments1[i], segments2[i]);
                if (comparsion_result > 0)
                    return true;
                else if (comparsion_result < 0)
                    return false;
            }

            //: common part are equivalent

            return segments1.Length > segments2.Length;
        }

        /// <summary>Invariant ignore case of paths comparer.</summary>
		public static bool operator <(PathInfo path1, PathInfo path2)
        {
            if ((object)path2 == null)
                return false;

            if ((object)path1 == null)
                return true;

            // compare path as string is incorrect, compare segments of path separately at each level of the hierarchy is the right way

            var segments1 = path1.Segments;
            var segments2 = path2.Segments;

            // compare common part of of these two paths

            for(int i = 0, c1 = segments1.Length, c2 = segments2.Length; i < c1 && i < c2; i++)
            {
                var comparsion_result = string.Compare(segments1[i], segments2[i]);
                if (comparsion_result < 0)
                    return true;
                else if (comparsion_result > 0)
                    return false;
            }

            //: common part are equivalent

            return segments1.Length < segments2.Length;
        }

        /// <summary>Invariant ignore case equality of paths comparer.</summary>
		public int CompareTo(PathInfo other)
		{
            if ((object)other == null || other.Empty)
                return (Empty) ? 0 : 1/*???*/;
            
            if (Empty)
                return -1/*???*/;

            // Compare path as string is incorrect, compare segments of path separately at each level of the hierarchy is the right way

            var segments1 = this.Segments;
            var segments2 = other.Segments;

            // compare common part of of these two paths

            for(int i = 0, c1 = segments1.Length, c2 = segments2.Length; i < c1 && i < c2; i++)
            {
                int level_compare_result = string.Compare(segments1[i], segments2[i]);
                if (level_compare_result != 0)
                    return level_compare_result;
            }

            //: common part are equivalent

            return segments1.Length - segments2.Length;
		}

        /// <summary>Invariant ignore case equality of paths comparer.
        /// Comparison to the string path is correct, but will turn a decrease in performance!
        /// Comparison to values of other types returns 0.
        /// </summary>
		public int CompareTo(object obj)
		{
			// Сравнивать нужно последовательно по фрагментам пути?
			// Пока так:

            if ((object)obj == null)
                return (Empty) ? 0 : 1/*???*/;
            
            if (Empty)
                return -1/*???*/;

			var other = obj as PathInfo;
            if ((object)other != null)
                return CompareTo(other);

            // You can compare PathInfo to strings, but will turn a significant decrease in performance!

            var other_path_string = obj as string;
            if ((object)other != null)
                return CompareTo(new PathInfo(other_path_string));
            
			return 0;
		}

        // Constructors

        /// <summary>Static factory method. Combine strings into a new PathInfo object.</summary>
        /// <param name="segments">Path segments</param>
        public static PathInfo Create(params string[] segments)
		{
			return new PathInfo(segments);
		}

        /// <summary>Combine PathInfo and strings into a new PathInfo object</summary>
        /// <param name="segments">Path segments</param>
		public PathInfo Combine(params string[] segments)
		{
			if ((object)segments == null || segments.Length == 0)
				return new PathInfo(Path);

            // precalc length for path buffer

			int len = (((object)_Path == null) ? 0 : _Path.Length);
			for (int i = 0, c = segments.Length; i < c; i++)
			{
				string seg = segments[i];

                // Allowed null path, null segment not allowed

                if ((object)seg == null || seg.Length == 0)
                    throw new ArgumentException("Path segment cannot be null or empty!");

				len += seg.Length + 1;
			}

            // build path

			var builder = new StringBuilder(len);
			bool opened = false;

			if ((object)_Path != null)
			{
				builder.Append(_Path);
				opened = true;
			}

			for (int i = 0, c = segments.Length; i < c; i++)
			{
				string seg = segments[i];

				if (opened)
					builder.Append(IOPath.DirectorySeparatorChar);

				builder.Append(seg);
				opened = true;
			}

			return new PathInfo(builder.ToString());
		}

        public PathInfo()
		{
		}

		public PathInfo(PathInfo path)
		{
            if (path.IsRelative)
                Base = new PathInfo(path.Base);

            Path = path.Path;
		}

		public PathInfo(string path)
		{
			Path = path;
		}

		public PathInfo(string segment1, string segment2)
		{
            if ((object)segment1 == null || segment1.Length == 0
            || (object)segment2 == null || segment2.Length == 0)
                throw new ArgumentException("Path segment cannot be null or empty!");

			Path = segment1 + IOPath.DirectorySeparatorChar + segment2;
		}

        public PathInfo(string segment1, string segment2, string segment3)
		{
			if ((object)segment1 == null || segment1.Length == 0
            || (object)segment2 == null || segment2.Length == 0
            || (object)segment3 == null || segment3.Length == 0)
                throw new ArgumentException("Path segment cannot be null or empty!");

			Path = segment1 + IOPath.DirectorySeparatorChar + segment2 + IOPath.DirectorySeparatorChar + segment3;
		}

        public PathInfo(string segment1, string segment2, string segment3, string segment4)
		{
			if ((object)segment1 == null || segment1.Length == 0
            || (object)segment2 == null || segment2.Length == 0
            || (object)segment3 == null || segment3.Length == 0
            || (object)segment4 == null || segment4.Length == 0)
                throw new ArgumentException("Path segment cannot be null or empty!");

			Path = segment1 + IOPath.DirectorySeparatorChar + segment2 + IOPath.DirectorySeparatorChar + segment3 + IOPath.DirectorySeparatorChar + segment4;
		}

        public PathInfo(params string[] segments)
		{
            // precalc length for path buffer

			int len = (((object)_Path == null) ? 0 : _Path.Length);
			for (int i = 0, c = segments.Length; i < c; i++)
			{
				string seg = segments[i];

                // Allowed null path, null segment not allowed

                if ((object)seg == null || seg.Length == 0)
                    throw new ArgumentException("Path segment cannot be null or empty!");

				len += seg.Length + 1;
			}

            // build path

			var builder = new StringBuilder(len);
			bool opened = false;
			for (int i = 0, c = segments.Length; i < c; i++)
			{
				if (opened)
					builder.Append(IOPath.DirectorySeparatorChar);

				builder.Append(segments[i]);
				opened = true;
			}

            Path = builder.ToString();
		}

        // Serialization

        public PathInfo(SerializationInfo info, StreamingContext context)
		{
            string _path = null, _base = null, _relative = null;
            foreach(var item in info)
            {
                switch(item.Name)
                {
                    case "Base":
                        _base = (string)item.Value;
                        break;
                    case "RelativePath":
                        _relative = (string)item.Value;
                        break;
                    case "Path":
                        _path = (string)item.Value;
                        break;
                }
            }

            if (_base != null || _relative != null)
            {
                Base = _base;
                Path = _relative;
            }
            else
            {
                Path = _path;
            }

        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
            if (IsRelative)
            {
                if ((object)_Base != null)
                    info.AddValue("Base", _Base.FullPath);
                if ((object)_Path != null)
                    info.AddValue("RelativePath", _Path);
            }
            else
            {
                if (FullPath != null)
                    info.AddValue("Path", FullPath);
            }
        }

        // XML serialization

        public XmlSchema GetSchema()
        {
            return null;
        }
        
        public void ReadXml(XmlReader reader)
        {
            var content = reader.ReadString();
            if (content != null)
            {
                Path = content;
            }
            else
            {
                var _base = reader.ReadElementContentAsString("Base", null);
                var _rel = reader.ReadElementContentAsString("RelativePath", null);
                if (_base != null || _rel != null)
                {
                    Base = _base;
                    Path = _rel;
                }
            }
        }
        
        public void WriteXml(XmlWriter writer)
        {
            if (IsRelative)
            {
                
                if ((object)_Base != null)
                    writer.WriteElementString("Base", _Base.FullPath);
                    
                if ((object)_Path != null)
                    writer.WriteElementString("RelativePath", _Path);
            }
            else
            {
                writer.WriteString(FullPath);
            }
        }
		
        // * ^ & child enumeration operators and other enumerators
               
        /// <summary>
        /// Returns an enumerable collection of file-system entries that match a search pattern in a specified path.
        /// </summary>
        /// <param name="search_pattern">The search string to match against the names of directories in path.</param>
        /// <exception cref="System.ArgumentException">path is a zero-length string, contains only white space, or contains invalid
        /// characters as defined by System.IO.Path.GetInvalidPathChars().- or -searchPattern
        /// does not contain a valid pattern.</exception>
        /// <exception cref="System.ArgumentNullException">path is null.-or-searchPattern is null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">path is invalid, such as referring to an unmapped drive.</exception>
        /// <exception cref="System.IO.IOException">path is a file name.</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum
        /// length. For example, on Windows-based platforms, paths must be less than
        /// 248 characters and file names must be less than 260 characters.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of file-system entries in the directory specified by path and that match search_pattern.</returns>
        public static PathList operator *(PathInfo path, string search_pattern)
        {
            return path .FileSystemEntries(search_pattern) .ToList();
        }

        public static PathList operator *(PathInfo path, Func<string,bool> match_comparer)
        {
            return path .FileSystemEntries(match_comparer) .ToList();
        }

        /// <summary>
        /// Returns an enumerable collection of file-system entries that match a search pattern in a specified path.
        /// </summary>
        /// <param name="recursive">The value One of the values of the System.IO.SearchOption enumeration that specifies
        /// whether the search operation should include only the current directory or
        /// should include all subdirectories.The default value is System.IO.SearchOption.TopDirectoryOnly.</param>
        /// <exception cref="System.ArgumentException">path is a zero-length string, contains only white space, or contains invalid
        /// characters as defined by System.IO.Path.GetInvalidPathChars().- or -searchPattern
        /// does not contain a valid pattern.</exception>
        /// <exception cref="System.ArgumentNullException">path is null.-or-searchPattern is null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">path is invalid, such as referring to an unmapped drive.</exception>
        /// <exception cref="System.IO.IOException">path is a file name.</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum
        /// length. For example, on Windows-based platforms, paths must be less than
        /// 248 characters and file names must be less than 260 characters.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of file-system entries in the directory specified by path and that match search_pattern.</returns>
        public PathList FileSystemEntries(bool recursive = false)
        {
            if (!recursive)
                return Directory
                    .EnumerateFileSystemEntries(FullPath)
                    .Select(path => new PathInfo(path))
                    .ToList();

            return Directory
                .EnumerateFileSystemEntries(FullPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(path => new PathInfo(path))
                    .ToList();
        }

        /// <summary>
        /// Returns an enumerable collection of file-system entries that match a search pattern in a specified path.
        /// </summary>
        /// <param name="search_pattern">The search string to match against the names of directories in path.</param>
        /// <param name="recursive">The value One of the values of the System.IO.SearchOption enumeration that specifies
        /// whether the search operation should include only the current directory or
        /// should include all subdirectories.The default value is System.IO.SearchOption.TopDirectoryOnly.</param>
        /// <exception cref="System.ArgumentException">path is a zero-length string, contains only white space, or contains invalid
        /// characters as defined by System.IO.Path.GetInvalidPathChars().- or -searchPattern
        /// does not contain a valid pattern.</exception>
        /// <exception cref="System.ArgumentNullException">path is null.-or-searchPattern is null.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">path is invalid, such as referring to an unmapped drive.</exception>
        /// <exception cref="System.IO.IOException">path is a file name.</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or combined exceed the system-defined maximum
        /// length. For example, on Windows-based platforms, paths must be less than
        /// 248 characters and file names must be less than 260 characters.</exception>
        /// <exception cref="System.Security.SecurityException">The caller does not have the required permission.</exception>
        /// <exception cref="System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <returns>An enumerable collection of file-system entries in the directory specified by path and that match search_pattern.</returns>
        public PathList FileSystemEntries(string search_pattern, bool recursive = false)
        {
            if (search_pattern == null)
            {
                if (!recursive)
                    return Directory
                        .EnumerateFileSystemEntries(FullPath)
                        .Select(path => new PathInfo(path))
                    .ToList();

                search_pattern = "*";
            }

            return Directory
                .EnumerateFileSystemEntries(FullPath, search_pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(path => new PathInfo(path))
                    .ToList();
        }

        public PathList FileSystemEntries(Func<string,bool> match_comparer, bool recursive = false)
        {
            if (!recursive)
                return Directory
                    .EnumerateFileSystemEntries(FullPath)
                    .Where(path => match_comparer(path))
                    .Select(path => new PathInfo(path))
                .ToList();

            return Directory
                .EnumerateFileSystemEntries(FullPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(path => match_comparer(path))
                .Select(path => new PathInfo(path))
                    .ToList();
        }

        public static PathList operator ^(PathInfo path, string search_pattern)
        {
            return path .Directories(search_pattern) .ToList();
        }

        public static PathList operator ^(PathInfo path, Func<string,bool> match_comparer)
        {
            return path .Directories(match_comparer) .ToList();
        }

        public PathList Directories(bool recursive = false)
        {
            
            if (!recursive)
                return Directory
                    .EnumerateDirectories(FullPath)
                    .Select(path => new PathInfo(path))
                    .ToList();

            return Directory
                .EnumerateDirectories(FullPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(path => new PathInfo(path))
                .ToList();
        }

        public PathList Directories(string search_pattern, bool recursive = false)
        {
            if (search_pattern == null)
            {
                if (!recursive)
                    return Directory
                        .EnumerateDirectories(FullPath)
                        .Select(path => new PathInfo(path))
                    .ToList();

                search_pattern = "*";
            }

            return Directory
                .EnumerateDirectories(FullPath, search_pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(path => new PathInfo(path))
                    .ToList();
        }

        public PathList Directories(Func<string,bool> match_comparer, bool recursive = false)
        {

            if (!recursive)
                return Directory
                    .EnumerateDirectories(FullPath)
                    .Where(path => match_comparer(path))
                    .Select(path => new PathInfo(path))
                    .ToList();

            return Directory
                .EnumerateDirectories(FullPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(path => match_comparer(path))
                .Select(path => new PathInfo(path))
                    .ToList();
        }

        public static PathList operator &(PathInfo path, string search_pattern)
        {
            return path .Files(search_pattern) .ToList();
        }

        public static PathList operator &(PathInfo path, Func<string,bool> match_comparer)
        {
            return path .Files(match_comparer) .ToList();
        }

        public PathList Files(bool recursive = false)
        {
            if (!recursive)
                return Directory
                    .EnumerateFiles(FullPath)
                    .Select(path => new PathInfo(path))
                    .ToList();

            return Directory
                .EnumerateFiles(FullPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(path => new PathInfo(path))
                    .ToList();
        }

        public PathList Files(string search_pattern, bool recursive = false)
        {
            if (search_pattern == null)
            {
                if (!recursive)
                    return Directory
                        .EnumerateFiles(FullPath)
                        .Select(path => new PathInfo(path))
                    .ToList();

                search_pattern = "*";
            }

            return Directory
                .EnumerateFiles(FullPath, search_pattern, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Select(path => new PathInfo(path))
                    .ToList();
        }

        public PathList Files(Func<string,bool> match_comparer, bool recursive = false)
        {
            if (!recursive)
                return Directory
                    .EnumerateFiles(FullPath)
                    .Where(path => match_comparer(path))
                    .Select(path => new PathInfo(path))
                    .ToList();

            return Directory
                .EnumerateFiles(FullPath, "*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                .Where(path => match_comparer(path))
                .Select(path => new PathInfo(path))
                    .ToList();
        }

        // Other

        public void Delete(bool recursive = false)
		{
            if (Directory.Exists(FullPath))
                Directory.Delete(FullPath, recursive);
            else
			    File.Delete(Path);
		}
        

        public PathInfo NewUniqueName(string name_pattern, string extension)
        {
            return null;
        }

        public PathList NewUniqueNames(string name_pattern, string extension, int number)
        {
            return null;
        }

        public static string ValidateFileName(string _name)
		{
            // Преобразовать текст к корректному для имени файла значению, заменить спецсимволы

            StringBuilder name = new StringBuilder(_name);

            for(int i = name.Length - 1; i >= 0; i--)
            if (InvalidFileNameChars.Contains(name[i]))
                name[i] = '-';

            return name.ToString();
        }

		public static bool CheckFileNameValidity(string name)
		{
			if (string.IsNullOrWhiteSpace(name))
				return false;

			if (name.IndexOfAny(InvalidFileNameChars) >= 0)
				return false;

			return true;
		}

		
	}

    // TODO Full mapping of the System.IO.File class methods on the PathInfo.File* methods
    public partial class PathInfo
    {
		public PathInfo FileCopy(PathInfo destination_file_path, bool overwrite = false)
		{
            // Копирвоание файла, целевой путь - файл
			File.Copy(Path, destination_file_path, overwrite);
			return destination_file_path;
		}

        public PathInfo FileMove(PathInfo destination_file_path)
		{
            // Перенос файла, целевой путь - файл
			File.Move(Path, destination_file_path);
			return destination_file_path;
		}

        public void FileDelete()
		{
			File.Delete(Path);
		}

        /// <summary>(There are no exceptions!) Determines whether the specified file exists.</summary>
        /// <returns>true if the caller has the required permissions and path contains the name
        ///     of an existing file; otherwise, false. This method also returns false if
        ///     path is null, an invalid path, or a zero-length string. If the caller does
        ///     not have sufficient permissions to read the specified file, no exception
        ///     is thrown and the method returns false regardless of the existence of path.</returns>
		public bool FileExists(string file_name = null)
		{
			return (file_name == null) ? File.Exists(FullPath) : File.Exists(IOPath.Combine(FullPath, file_name));
		}

        /// <summary>Gets a System.Security.AccessControl.FileSecurity object that encapsulates
        /// the specified type of access control list (ACL) entries for a particular file.
        /// 
        /// </summary>
        /// <param name="include_sections">One of the System.Security.AccessControl.AccessControlSections values that
        /// specifies the type of access control list (ACL) information to receive.</param>
        /// <returns>A System.Security.AccessControl.FileSecurity object that encapsulates the
        ///     access control rules for the file described by the path parameter.</returns>
        ///     <exception cref="System.IO.IOException">An I/O error occurred while opening the file.</exception>
        ///     <exception cref="System.Runtime.InteropServices.SEHException"></exception>
        ///     <exception cref="The path is null."></exception>
        ///     <exception cref="System.SystemException">The file could not be found.</exception>
        ///     <exception cref="System.UnauthorizedAccessException">The path parameter specified a file that is read-only.-or- This operation
        /// is not supported on the current platform.-or- The path parameter specified
        /// a directory.-or- The caller does not have the required permission.</exception>
        public FileSecurity FileGetAccessControl(AccessControlSections include_sections = AccessControlSections.All)
        {
            return File.GetAccessControl(FullPath, include_sections);
        }

        public FileStream FileOpen(FileMode mode = FileMode.Open, FileAccess access = FileAccess.Read, FileShare share = FileShare.Read)
		{
			return File.Open(Path, mode, access, share);
		}
    }

    // TODO Additional File methods.
    public partial class PathInfo
    {
        /// <summary>(The method does not throw any exceptions!)
        /// 
        /// </summary>
        /// <returns></returns>
        public bool TryFileDelete()
		{
            try
            {
			    File.Delete(Path);
			    return true;
            }
            catch
            {
            }

            return false;
		}
    }

    // TODO Full mapping of the System.IO.Directory class methods on the PathInfo.Directory* methods.
    public partial class PathInfo
    {
        // TODO
        // This example. It is also necessary to copy-paste and other Directory.methods..

        /// <summary>Creates all directories and subdirectories in the specified path.</summary>
        /// <exception cref="System.IO.IOException">The directory specified by path is a file .-or-The network name is not known.</exception>
        /// <exception cref="System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="System.ArgumentException">path is a zero-length string, contains only white space, or contains one
        /// or more invalid characters as defined by System.IO.Path.InvalidPathChars.-or-path
        /// is prefixed with, or contains only a colon character (:).</exception>
        /// <exception cref="System.ArgumentNullException">path is null.</exception>
        /// <exception cref="System.IO.PathTooLongException"> The specified path, file name, or both exceed the system-defined maximum
        /// length. For example, on Windows-based platforms, paths must be less than
        /// 248 characters and file names must be less than 260 characters.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">The specified path is invalid (for example, it is on an unmapped drive).</exception>
        /// <exception cref="System.NotSupportedException">path contains a colon character (:) that is not part of a drive label ("C:\").</exception>
        /// <returns>Path of created directory with assigned DirectoryInfo property.</returns>
        public PathInfo DirectoryCreate(DirectorySecurity directory_security = null)
		{
			if (directory_security != null)
				_DirectoryInfo = Directory.CreateDirectory(FullPath, directory_security);
            else
                _DirectoryInfo = Directory.CreateDirectory(FullPath);

            directory_info_retrieved = true;

            return this;
		}

        public PathInfo DirectoryCreate(FileAttributes attributes, DirectorySecurity directory_security = null)
		{
            if (directory_security != null)
				_DirectoryInfo = Directory.CreateDirectory(FullPath, directory_security);
            else
                _DirectoryInfo = Directory.CreateDirectory(FullPath);

            directory_info_retrieved = true;

            if (attributes != FileAttributes.Directory)
                _DirectoryInfo.Attributes = attributes | FileAttributes.Directory;

            return this;
		}

        public PathInfo DirectoryCreate(string subdirectory, DirectorySecurity directory_security = null)
		{
			return Combine(subdirectory).DirectoryCreate(directory_security);
		}

        public PathInfo DirectoryCreate(string subdirectory, FileAttributes attributes, DirectorySecurity directory_security = null)
		{
            return Combine(subdirectory).DirectoryCreate(attributes, directory_security);
		}

        /// <summary>Deletes the specified directory and, if indicated, any subdirectories and files in the directory.</summary>
        /// 
        /// <exception cref="System.IO.IOException">A file with the same name and location specified by path exists.-or-The directory
        /// specified by path is read-only, or recursive is false and path is not an
        /// empty directory. -or-The directory is the application's current working directory.
        /// -or-The directory contains a read-only file.-or-The directory is being used
        /// by another process.There is an open handle on the directory or on one of
        /// its files, and the operating system is Windows XP or earlier. This open handle
        /// can result from enumerating directories and files. For more information,
        /// see How to: Enumerate Directories and Files.</exception>
        /// 
        /// <exception cref="System.UnauthorizedAccessException">The caller does not have the required permission.</exception>
        /// <exception cref="System.ArgumentException">path is a zero-length string, contains only white space, or contains one
        /// or more invalid characters as defined by System.IO.Path.InvalidPathChars.</exception>
        /// <exception cref="System.ArgumentNullException">path is null</exception>
        /// <exception cref="System.IO.PathTooLongException">The specified path, file name, or both exceed the system-defined maximum
        /// length. For example, on Windows-based platforms, paths must be less than
        /// 248 characters and file names must be less than 260 characters.</exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">path does not exist or could not be found.-or-path refers to a file instead
        /// of a directory.-or-The specified path is invalid (for example, it is on an
        /// unmapped drive).</exception>
        /// 
        /// <param name="recursive">true to remove directories, subdirectories, and files in path; otherwise, false.</param>
        public void DirectoryDelete(bool recursive = false)
        {
            Directory.Delete(FullPath, recursive);
        }

        public void DirectoryDelete(string subdirectory, bool recursive = false)
        {
            Directory.Delete(IOPath.Combine(FullPath, subdirectory), recursive);
        }

        /// <summary>
        /// (There are exceptions!)Determines whether the given path refers to an existing directory on disk.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">The path parameter is null.</exception>
        /// <exception cref="System.UnauthorizedAccessException">??? todo</exception>
        /// <seealso cref="TryDirectoryExists"/>
        /// <returns>true if path refers to an existing directory; otherwise, false.</returns>
		public bool DirectoryExists(string subdirectory = null)
		{
            return (subdirectory == null) ? Directory.Exists(FullPath) : Directory.Exists(IOPath.Combine(FullPath, subdirectory));
		}

        /// <summary>Gets a System.Security.AccessControl.DirectorySecurity object that encapsulates 
        /// the specified type of access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="include_sections">One of the System.Security.AccessControl.AccessControlSections values that
        /// specifies the type of access control list (ACL) information to receive.</param>
        /// <exception cref="System.ArgumentNullException">The path parameter is null.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while opening the directory.</exception>
        /// <exception cref="System.PlatformNotSupportedException">The current operating system is not Windows 2000 or later.</exception>
        /// <exception cref="System.SystemException">The directory could not be found.</exception>
        /// <exception cref="System.UnauthorizedAccessException">The path parameter specified a directory that is read-only.-or- This operation
        /// is not supported on the current platform.-or- The caller does not have the
        /// required permission.</exception>
        /// <returns>A System.Security.AccessControl.DirectorySecurity object that encapsulates
        /// the access control rules for the file described by the path parameter.</returns>
        public DirectorySecurity DirectoryGetAccessControl(AccessControlSections include_sections = AccessControlSections.All)
		{
            return Directory.GetAccessControl(Path, include_sections);
		}
    }

    // TODO Additional Directory methods.
    public partial class PathInfo
    {
        /// <summary>(The method does not throw any exceptions!)
        /// 
        /// </summary>
        /// <returns></returns>
        public bool TryDirectoryCreate(DirectorySecurity directory_security = null)
		{
			if (Directory.Exists(FullPath))
                return true;

			try
			{
                if (directory_security != null)
				    _DirectoryInfo = Directory.CreateDirectory(FullPath, directory_security);
                else
                    _DirectoryInfo = Directory.CreateDirectory(FullPath);

                directory_info_retrieved = true;

				return true;
			}
			catch
            {
            }

			return false;
		}

        /// <summary>(The method does not throw any exceptions!)
        /// 
        /// </summary>
        /// <returns></returns>
        public bool TryDirectoryCreate(FileAttributes attributes, DirectorySecurity directory_security = null)
		{
			if (Directory.Exists(FullPath))
                return true;

			try
			{
                if (directory_security != null)
				    _DirectoryInfo = Directory.CreateDirectory(FullPath, directory_security);
                else
                    _DirectoryInfo = Directory.CreateDirectory(FullPath);

                directory_info_retrieved = true;

                if (attributes != FileAttributes.Directory)
                    _DirectoryInfo.Attributes = attributes | FileAttributes.Directory;

				return true;
			}
			catch
            {
            }

			return false;
		}

        public bool TryDirectoryCreate(string subdirectory, DirectorySecurity directory_security = null)
		{
            return Combine(subdirectory).TryDirectoryCreate(directory_security);
		}

        public bool TryDirectoryCreate(string subdirectory, FileAttributes attributes, DirectorySecurity directory_security = null)
		{
			return Combine(subdirectory).TryDirectoryCreate(attributes, directory_security);
		}

        /// <summary>(The method does not throw any exceptions!) Determines whether the given path refers to an existing directory on disk.</summary>
        /// <returns>true if the caller has the required permissions and path contains the name
        /// of an existing directory; otherwise, false. This method also returns false if
        /// path is null, an invalid path, or a zero-length string. If the caller does
        /// not have sufficient permissions to read the specified path, no exception
        /// is thrown and the method returns false regardless of the existence of path.</returns>
        public bool TryDirectoryExists()
		{
            try
            {
			    return Directory.Exists(FullPath);
            }
            catch
            {
            }

            return false;
		}

        public bool TryDirectoryExists(string subdirectory)
		{
            try
            {
			    return Directory.Exists(IOPath.Combine(FullPath, subdirectory));
            }
            catch
            {
            }

            return false;
		}

    }
    
    public class PathList : List<PathInfo>
    {
        public static PathList operator +(PathList paths1, IEnumerable<PathInfo> paths2)
        {
            var list  = new PathList(paths1.Count + paths2.Count());
            list.AddRange(paths1);
            list.AddRange(paths2);
            return list;
        }

        public static PathList operator -(PathList paths1, IEnumerable<PathInfo> paths2)
        {
            var list  = new PathList(paths1);
            
            foreach(var path2 in paths2)
            {
                var index = list.IndexOf(path2);
                if (index >= 0)
                    list.RemoveAt(index);
            }

            return list;
        }

        public static PathList operator -(PathList paths1, string search_pattern)
        {
            var list  = new PathList(paths1);

            for(int i = paths1.Count - 1; i >= 0; i--)
            {
                var path = paths1[i];

                // ...

                list.Remove(path);
            }
            
            return list;
        }

        public static implicit operator PathList(string[] paths)
        {
            return new PathList(paths);
        }

        public PathList()
        {
        }

        public PathList(IEnumerable<PathInfo> paths) : base(paths)
        {
        }

        public PathList(IEnumerable<string> paths) : base(paths.Select(path => new PathInfo(path)))
        {
        }

        public PathList(int capacity) : base(capacity)
        {
        }
    }

    public static class PathEnumerable
    {
        public static PathList ToList(this IEnumerable<PathInfo> paths)
        {
            var list  = new PathList(paths.Count());
            list.AddRange(paths);
            return list;
        }
    }

    /// <summary>Safe walker with error handling and cancellation token</summary>
    public class PathWalker
    {
        // TODO
    }

    #region    ------------------ Bulk Pattern Origin ------------------

    /* Основная суть этого паттерна получить групповой результат выполнения операции над перечислимым множеством для последующей обработки ошибок или успехов пакетной операцией.
     * Паттерн еще не закончен в том смысле, что нужно еще добавить устоявшийся способ группировки результатов множества операций над одним или несколькими перечислимыми множествами.
     * 
     * The main essence of this pattern have group result of the operation of the enumerable set for later processing error or success batch operation.
     * The pattern is not yet complete in the sense that it is necessary to add an method of grouping results of multiple operations involving one or more enumerated sets.
     */

    public class WrapException<T> : Exception
    {
        public T Object;

        public WrapException(Exception e, T obj) : base("Wrapped: " + e.Message, e)
        {
            Object = obj;
        }
    }

    public class BulkException<T> : Exception
    {
        public IList<T> Successful;
        public IList<WrapException<T>> Failed;

        public BulkException()
        {
            Successful = new List<T>();
            Failed =  new List<WrapException<T>>();
        }

        public BulkException(IList<T> successful, IList<WrapException<T>> failed)
        {
            Successful = successful;
            Failed = failed;
        }
    }

    public static class BulkEnumerable
    {
        public static void Bulk<T>(this IEnumerable<T> paths, Action<T> action)
        {
            List<T> successful = new List<T>();
            List<WrapException<T>> failed = null;

            foreach(var path in paths)
            {
                try
                {
                    action(path);
                    successful.Add(path);
                }
                catch (Exception e)
                {
                    if (failed == null)
                        failed = new List<WrapException<T>>();

                    failed.Add(new WrapException<T>(e, path));
                }
            }

            if (failed != null)
                throw new BulkException<T>(successful, failed);
        }
    }

    #endregion ------------------ Bulk Pattern Origin ------------------
}