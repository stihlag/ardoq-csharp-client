﻿using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Ardoq;
using Ardoq.Models;
using Ardoq.Service.Interface;
using ArdoqTest.Helper;
using NUnit.Framework;

namespace ArdoqTest.Service
{
    [TestFixture]
    public class ReferenceServiceTest
    {
        private IReferenceService service;
        private IArdoqClient client;

        [TestFixtureSetUp]
        public void Setup()
        {
            client = TestUtils.GetClient();
            service = client.ReferenceService;
        }

        private async Task DeleteWorkspace(Workspace workspace)
        {
            await client.WorkspaceService.DeleteWorkspace(workspace.Id, client.Org);
        }

        private async Task<Workspace> CrateWorkspace()
        {
            return
                await
                    client.WorkspaceService.CreateWorkspace(new Workspace("Reference Test Workspace",
                        TestUtils.GetTestPropery("modelId"), "Hello world!"), client.Org);
        }

        private async Task<Component> CreateComponent(Workspace workspace, string name)
        {
            return await client.ComponentService.CreateComponent(new Component(name, workspace.Id, ""), client.Org);
        }

        private Reference CreateReferenceTemplate(Workspace workspace, Component source, Component target)
        {
            return new Reference(workspace.Id, "", source.Id, target.Id, 2);
        }

        [Test]
        public async void CreateReferenceTest()
        {
            Workspace workspace = await CrateWorkspace();
            Component source = await CreateComponent(workspace, "Source");
            Component target = await CreateComponent(workspace, "Target");
            Reference referenceTemplate = CreateReferenceTemplate(workspace, source, target);
            Reference reference = await service.CreateReference(referenceTemplate, client.Org);
            Assert.NotNull(reference.Id);
            await DeleteWorkspace(workspace);
        }

        [Test]
        public async void DeleteReferenceTest()
        {
            Workspace workspace = await CrateWorkspace();
            Component source = await CreateComponent(workspace, "Source");
            Component target = await CreateComponent(workspace, "Target");
            Reference referenceTemplate = CreateReferenceTemplate(workspace, source, target);
            Reference result = await service.CreateReference(referenceTemplate, client.Org);
            await service.DeleteReference(result.Id, client.Org);

            try
            {
                await service.GetReferenceById(result.Id, client.Org);
                await DeleteWorkspace(workspace);
                Assert.Fail("Expected the reference to be deleted.");
            }
            catch (HttpRequestException)
            {
            }
            await DeleteWorkspace(workspace);
        }

        [Test]
        public async void GetReferenceTest()
        {
            Workspace workspace = await CrateWorkspace();
            Component source = await CreateComponent(workspace, "Source");
            Component target = await CreateComponent(workspace, "Target");
            Reference referenceTemplate = CreateReferenceTemplate(workspace, source, target);

            // fill the list 
            await service.CreateReference(referenceTemplate, client.Org);
            List<Reference> allWorkspaces = await service.GetAllReferences(client.Org);
            string id = allWorkspaces[0].Id;
            Reference reference = await service.GetReferenceById(id, client.Org);
            Assert.True(id == reference.Id);
            await DeleteWorkspace(workspace);
        }

        [Test]
        public async void UpdateReferenceTest()
        {
            Workspace workspace = await CrateWorkspace();
            Component source = await CreateComponent(workspace, "Source");
            Component target = await CreateComponent(workspace, "Target");
            Reference referenceTemplate = CreateReferenceTemplate(workspace, source, target);
            Reference reference = await service.CreateReference(referenceTemplate, client.Org);
            reference.Source = reference.Target;
            reference.Target = reference.Source;
            Reference updatedReference = await service.UpdateReference(reference.Id, reference, client.Org);
            Assert.True(reference.Target == updatedReference.Source);
            Assert.True(reference.VersionCounter == updatedReference.VersionCounter - 1);
            await DeleteWorkspace(workspace);
        }
    }
}