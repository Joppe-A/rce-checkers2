// Copyright 2013 Cultural Heritage Agency of the Netherlands, Dutch National Military Museum and Trezorix bv
// 
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
// 
//        http://www.apache.org/licenses/LICENSE-2.0
// 
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Trezorix.Common.IO;

using Path = System.IO.Path;

namespace Trezorix.ResourceRepository
{
	public class ResourceRepository<TEntity> : IResourceRepository<TEntity>
	{
		private readonly string _repositoryPath;

		public ResourceRepository(string repositoryPath)
		{
			if (string.IsNullOrEmpty(repositoryPath)) throw new ArgumentNullException("repositoryPath");

			if (!Directory.Exists(repositoryPath))
			{
				throw new ApplicationException("Repository base path doesn't exist, can't start or open repository. Configured path: " + _repositoryPath);
			}

			_repositoryPath = repositoryPath;
		}

		public virtual Resource<TEntity> Get(string id)
		{
			string fileSettings = MakeDocumentSettingFilePathName(id);
			if (!File.Exists(fileSettings))
			{
				return null;
			}
			try
			{
				return RetryingFileOperation.Attempt(() => LoadEntity(fileSettings));
			}
			catch (Exception ex)
			{
				throw new ResourceRepositoryException(String.Format("Can't read config file for ID: {0}, file may be corrupt: ", id) + fileSettings, ex);
			}
		}

		private Resource<TEntity> LoadEntity(string fileSettings)
		{
			using (var sr1 = new FileStream(fileSettings, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var xs1 = new DataContractSerializer(typeof(Resource<TEntity>));
				var readResource1 = (Resource<TEntity>)xs1.ReadObject(sr1);
				readResource1.SetPath(MakeSourcePath(readResource1.Id));

				return readResource1;
			}
		}

		public Resource<TEntity> Create(TEntity entity)
		{
			var resource = CreateResource(entity);

			Add(resource);

			return resource;
		}

		public Resource<TEntity> Create(TEntity entity, string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");

			fileName = Path.GetFileName(fileName);

			var resource = CreateResource(entity);

			resource.FileName = fileName;

			resource.Entity = entity;

			Add(resource);

			return resource;
		}

		private Resource<TEntity> CreateResource(TEntity entity)
		{
			// claim new ID
			var newId = Guid.NewGuid().ToString();
			return new Resource<TEntity>(entity, MakeSourcePath(newId))
				   {
					   Id = newId,
					   CreationDate = DateTime.UtcNow
				   };
		}

		public virtual void Add(Resource<TEntity> entity)
		{
			// ToDo: Joppe: Check if this entity isn't already known

			// create storage folder for the files
			Directory.CreateDirectory(MakeSourcePath(entity.Id));
			Directory.CreateDirectory(entity.ArtifactFolder);

			RetryingFileOperation.Attempt(() => SaveEntity(entity));
		}

		private void SaveEntity(Resource<TEntity> entity)
		{
			using (var fs = new FileStream(MakeDocumentSettingFilePathName(entity.Id), FileMode.Create, FileAccess.Write, FileShare.None))
			{
				var xs = new DataContractSerializer(typeof(Resource<TEntity>));

				entity.ModificationDate = DateTime.UtcNow;

				xs.WriteObject(fs, entity);
				fs.Flush(true);
				fs.Close();
			}
		}

		public IEnumerable<Resource<TEntity>> All()
		{
			var result = Directory.EnumerateFiles(_repositoryPath, "*.xml").Select(Path.GetFileNameWithoutExtension).Select(Get);
			return result;
		}

		public string RepositoryPath
		{
			get { return _repositoryPath; }
		}

		public virtual void Update(Resource<TEntity> entity)
		{
			// ToDo: Joppe: Check for existence of entity first?
			RetryingFileOperation.Attempt(() => SaveEntity(entity));
		}

		private string MakeDocumentSettingFilePathName(string id)
		{
			return Path.Combine(_repositoryPath, id + @".xml");
		}

		protected virtual string MakeSourcePath(string id)
		{
			return Path.Combine(_repositoryPath, id + @"\");
		}

		public void Delete(Resource<TEntity> entity)
		{
			try
			{
				DeleteArtifacts(entity);

				DeleteDirectoryWhenExists(entity.ArtifactFolder);

				// ToDo: Leaky abstraction? Profiles don't save a file of their own
				if (!string.IsNullOrEmpty(entity.FileName))
				{
					DeleteFileWhenExists(entity.FilePathName());
				}

				DeleteDirectoryWhenExists(MakeSourcePath(entity.Id));
			}
			catch
			{
				; // ignore any deletion errors up to here
				// ToDo: catch explicitly...
			}

			// do make sure that the settings file is deleted
			DeleteFileWhenExists(MakeDocumentSettingFilePathName(entity.Id));
		}

		public void Delete(string id)
		{
			var doc = Get(id);
			if (doc == null) return;

			Delete(doc);
			
		}

		public void DeleteArtifacts(string id)
		{
			var doc = Get(id);
			if (doc == null) return;
			DeleteArtifacts(doc);
			Update(doc);
		}

		private static void DeleteArtifacts(Resource<TEntity> document)
		{
			foreach (var artifact in document.Artifacts)
			{
				DeleteFileWhenExists(artifact.FilePathName);
			}
		}

		private static void DeleteFileWhenExists(string filePathName)
		{
			if (File.Exists(filePathName)) File.Delete(filePathName);
		}

		private static void DeleteDirectoryWhenExists(string path)
		{
			if (Directory.Exists(path)) Directory.Delete(path, true);
		}

		public virtual void Clear()
		{
			foreach (var resource in All())
			{
				Delete(resource.Id);
			}	
		}

	}

	public class ResourceRepositoryException : ApplicationException
	{
		public ResourceRepositoryException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		public ResourceRepositoryException(string message)
			: base(message)
		{
		}
	}
}