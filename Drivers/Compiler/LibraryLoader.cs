#region LICENSE

// ---------------------------------- LICENSE ---------------------------------- //
//
//    Fling OS - The educational operating system
//    Copyright (C) 2015 Edward Nutting
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
//  Project owner: 
//		Email: edwardnutting@outlook.com
//		For paper mail address, please contact via email for details.
//
// ------------------------------------------------------------------------------ //

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Drivers.Compiler.IL;
using Drivers.Compiler.Types;

namespace Drivers.Compiler
{
    /// <summary>
    ///     Manages loading libraries from files (e.g. IL libraries from .dll or .exe files).
    /// </summary>
    public static class LibraryLoader
    {
        /// <summary>
        ///     Library caching means only one ILLibrary object used per assembly (even if multiple refs exist in
        ///     different dependency locations) so type scanner, compiler etc. don't duplicate work later.
        /// </summary>
        private static readonly Dictionary<string, ILLibrary> LibraryCache = new Dictionary<string, ILLibrary>();

        /// <summary>
        ///     Loads an IL Library from the specified file.
        /// </summary>
        /// <param name="FilePath">The path to the file to load as an IL Library.</param>
        /// <returns>The loaded IL Library.</returns>
        private static ILLibrary LoadILLibraryFromFile(string FilePath)
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                Logger.LogError(Errors.Loader_LibraryPathNullOrEmpty_ErrorCode, FilePath, 0,
                    Errors.ErrorMessages[Errors.Loader_LibraryPathNullOrEmpty_ErrorCode]);

                return null;
            }
            if (!File.Exists(FilePath))
            {
                Logger.LogError(Errors.Loader_LibraryFileDoesntExist_ErrorCode, FilePath, 0,
                    Errors.ErrorMessages[Errors.Loader_LibraryFileDoesntExist_ErrorCode]);

                return null;
            }

            try
            {
                ILLibrary result = new ILLibrary();
                result.TheAssembly = Assembly.LoadFrom(FilePath);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(Errors.Loader_UnexpectedError_ErrorCode, FilePath, 0,
                    string.Format(
                        Errors.ErrorMessages[Errors.Loader_UnexpectedError_ErrorCode],
                        ex.Message, ex.StackTrace));
            }
            return null;
        }

        /// <summary>
        ///     Loads an IL Library from the specified file and scans types within the library.
        /// </summary>
        /// <param name="FilePath">The path to the file to load as an IL Library.</param>
        /// <returns>The loaded IL Library.</returns>
        public static ILLibrary LoadILLibrary(string FilePath)
        {
            ILLibrary TheLibrary = LoadILLibraryFromFile(FilePath);

            if (TheLibrary != null)
            {
                LoadDependencies(TheLibrary);

                TypeScanner.ScanTypes(TheLibrary);
            }

            return TheLibrary;
        }

        /// <summary>
        ///     Loads all the dependencies of a library.
        /// </summary>
        /// <param name="aLibrary">The library to load dependencies of.</param>
        /// <returns>The number of dependencies loaded.</returns>
        public static int LoadDependencies(ILLibrary aLibrary)
        {
            if (aLibrary == null)
            {
                return 0;
            }

            int DependenciesLoaded = 0;

            Assembly RootAssembly = aLibrary.TheAssembly;
            AssemblyName[] refAssemblyNames = RootAssembly.GetReferencedAssemblies();
            foreach (AssemblyName aRefName in refAssemblyNames)
            {
                if (IsAssemblyFullNameIgnored(aRefName.FullName))
                {
                    continue;
                }

                string refFilePath = Path.Combine(Path.GetDirectoryName(RootAssembly.Location), aRefName.Name + ".dll");
                ILLibrary refLibrary = LoadILLibrary(refFilePath);
                if (refLibrary == null)
                {
                    throw new NullReferenceException("Loaded dependency library was null!");
                }
                if (LibraryCache.ContainsKey(refLibrary.ToString()))
                {
                    aLibrary.Dependencies.Add(LibraryCache[refLibrary.ToString()]);
                }
                else
                {
                    aLibrary.Dependencies.Add(refLibrary);
                    LibraryCache.Add(refLibrary.ToString(), refLibrary);
                }
            }

            return DependenciesLoaded;
        }

        /// <summary>
        ///     Determines whether the assembly name is in the list of names to ignore.
        /// </summary>
        /// <param name="fullName">The name to test.</param>
        /// <returns>True if it is in the ignore list, otherwise false.</returns>
        public static bool IsAssemblyFullNameIgnored(string fullName)
        {
            foreach (string ignoreName in Options.IgnoreAssemblies)
            {
                if (fullName.Contains(ignoreName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}