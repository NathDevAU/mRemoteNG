﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using mRemoteNG.App.Info;
using mRemoteNG.Config.DataProviders;
using mRemoteNG.Config.Serializers;
using mRemoteNG.Config.Serializers.Versioning;
using mRemoteNG.Credential;
using mRemoteNG.Tools;
using mRemoteNG.Tree;
using mRemoteNG.UI.Forms;

namespace mRemoteNG.Config.Connections
{
    public class XmlConnectionsLoader
    {
        private readonly string _connectionFilePath;
        private readonly IEnumerable<ICredentialRecord> _credentialRecords;
        private readonly string _credentialFilePath = Path.Combine(CredentialsFileInfo.CredentialsPath, CredentialsFileInfo.CredentialsFile);

        public XmlConnectionsLoader(string connectionFilePath, IEnumerable<ICredentialRecord> credentialRecords)
        {
            if (string.IsNullOrEmpty(connectionFilePath))
                throw new ArgumentException($"{nameof(connectionFilePath)} cannot be null or empty");
            if (credentialRecords == null)
                throw new ArgumentNullException(nameof(credentialRecords));

            _connectionFilePath = connectionFilePath;
            _credentialRecords = credentialRecords;
        }

        public ConnectionTreeModel Load()
        {
            var dataProvider = new FileDataProvider(_connectionFilePath);
            var xmlString = dataProvider.Load();
            var credServiceFactory = new CredentialServiceFactory();
            var deserializer = new CredentialManagerUpgradeForm
            {
                ConnectionFilePath = _connectionFilePath,
                NewCredentialRepoPath = _credentialFilePath,
                DecoratedDeserializer = new XmlCredentialManagerUpgrader(
                    credServiceFactory.Build(),
                    _credentialFilePath,
                    new XmlConnectionsDeserializer(_credentialRecords, PromptForPassword)
                )
            };
            return deserializer.Deserialize(xmlString);
        }

        private SecureString PromptForPassword()
        {
            var password = MiscTools.PasswordDialog("", false);
            return password;
        }
    }
}
