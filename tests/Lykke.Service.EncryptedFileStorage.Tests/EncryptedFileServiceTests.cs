using System;
using System.Threading.Tasks;
using AutoFixture;
using Lykke.Logs;
using Lykke.Service.EncryptedFileStorage.Domain.Models;
using Lykke.Service.EncryptedFileStorage.Domain.Repositories;
using Lykke.Service.EncryptedFileStorage.DomainServices;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Storage.Blob;
using Moq;
using Xunit;

namespace Lykke.Service.EncryptedFileStorage.Tests
{
    public class EncryptedFileServiceTests
    {
        private readonly Fixture _fixture = new Fixture();

        private readonly Mock<IEncryptedFileInfoRepository> _encryptedFileRepositoryMock =
            new Mock<IEncryptedFileInfoRepository>();

        private readonly Mock<IEncryptedFileContentRepository> _encryptedFileContentRepositoryMock =
            new Mock<IEncryptedFileContentRepository>();

        private readonly EncryptedFileService _service;

        public EncryptedFileServiceTests()
        {
            _service = new EncryptedFileService(
                _encryptedFileRepositoryMock.Object, 
                _encryptedFileContentRepositoryMock.Object,
                EmptyLogFactory.Instance);
        }

        [Fact]
        public async Task When_Store_And_Encrypt_File_Async_Is_Then_File_Repository_Is_Called()
        {
            _encryptedFileRepositoryMock.Setup(x => x.CreateFileInfoAsync(It.IsAny<EncryptedFile>()))
                .Returns(Task.FromResult(Guid.NewGuid()));

            _encryptedFileRepositoryMock.Setup(x =>
                    x.FileInfoByOriginAndFileNameExistsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.FromResult(false));

            await _service.CreateFileInfoAsync(_fixture.Create<EncryptedFile>());

            _encryptedFileRepositoryMock.Verify(x => x.CreateFileInfoAsync(It.IsAny<EncryptedFile>()), Times.Once);
        }

        [Fact]
        public async Task When_Get_File_Info_Async_Is_Executed_Then_File_Repository_Is_Called()
        {
            _encryptedFileRepositoryMock.Setup(x => x.GetFileInfoAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(_fixture.Create<EncryptedFile>()));

            await _service.GetFileMetadataAsync(It.IsAny<Guid>());

            _encryptedFileRepositoryMock.Verify(x => x.GetFileInfoAsync(It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Delete_File_Async_Is_Executed_And_File_Info_Deletion_Failed_Then_Only_File_Repository_Is_Called()
        {
            _encryptedFileRepositoryMock.Setup(x => x.DeleteFileInfoAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(false));

            _encryptedFileRepositoryMock.Setup(x => x.GetFileInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new EncryptedFile {Origin = "Test"});

            await _service.DeleteFileAsync(It.IsAny<Guid>());

            _encryptedFileRepositoryMock.Verify(x => x.DeleteFileInfoAsync(It.IsAny<Guid>()), Times.Once);
            _encryptedFileContentRepositoryMock.Verify(x => x.DeleteContentAsync(It.IsAny<EncryptedFile>()), Times.Never);
        }

        [Fact]
        public async Task
            When_Delete_File_Async_Is_Executed_And_File_Info_Deletion_Went_Ok_Then_FileAnd_File_Content_Repositories_Are_Called()
        {
            _encryptedFileRepositoryMock.Setup(x => x.DeleteFileInfoAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(true));

            _encryptedFileContentRepositoryMock.Setup(x => x.DeleteContentAsync(It.IsAny<EncryptedFile>()))
                .Returns(Task.CompletedTask);

            _encryptedFileRepositoryMock.Setup(x => x.GetFileInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new EncryptedFile { Origin = "Test", IsUploadCompleted = true });

            await _service.DeleteFileAsync(It.IsAny<Guid>());

            _encryptedFileRepositoryMock.Verify(x => x.DeleteFileInfoAsync(It.IsAny<Guid>()), Times.Once);
            _encryptedFileContentRepositoryMock.Verify(x => x.DeleteContentAsync(It.IsAny<EncryptedFile>()), Times.Once);
        }

        [Fact]
        public async Task
            When_Delete_File_Async_Is_Executed_And_File_Info_Deletion_Went_Ok_Then_File_Info_DeletedAnd_File_Content_Repositories_Not_Called()
        {
            _encryptedFileRepositoryMock.Setup(x => x.DeleteFileInfoAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(true));

            _encryptedFileContentRepositoryMock.Setup(x => x.DeleteContentAsync(It.IsAny<EncryptedFile>()))
                .Returns(Task.CompletedTask);

            _encryptedFileRepositoryMock.Setup(x => x.GetFileInfoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new EncryptedFile { Origin = "Test", IsUploadCompleted = false });

            await _service.DeleteFileAsync(It.IsAny<Guid>());

            _encryptedFileRepositoryMock.Verify(x => x.DeleteFileInfoAsync(It.IsAny<Guid>()), Times.Once);
            _encryptedFileContentRepositoryMock.Verify(x => x.DeleteContentAsync(It.IsAny<EncryptedFile>()), Times.Never);
        }

        [Fact]
        public async Task
            When_Get_Paginated_Files_Info_Async_Is_Executed_With_Invalid_Current_Page_Then_Exception_Is_Thrown()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPaginatedFilesMetadataAsync(0, 1));

            Assert.Equal("currentPage", ex.ParamName);
        }

        [Fact]
        public async Task
            When_Get_Paginated_Files_Info_Async_Is_Executed_With_Invalid_Page_Size_Then_Exception_Is_Thrown()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _service.GetPaginatedFilesMetadataAsync(1, 0));

            Assert.Equal("pageSize", ex.ParamName);
        }

        [Fact]
        public async Task
            When_Get_Paginated_Files_Info_Async_Is_Executed_Then_File_Repository_Is_Called()
        {
            _encryptedFileRepositoryMock.Setup(x => x.GetPaginatedFilesInfoAsync(It.IsAny<int>(), It.IsAny<int>()))
                .Returns(Task.FromResult(_fixture.CreateMany<EncryptedFile>()));

            _encryptedFileRepositoryMock.Setup(x => x.GetTotalAsync()).Returns(Task.FromResult(50));

            await _service.GetPaginatedFilesMetadataAsync(1, 10);

            _encryptedFileRepositoryMock.Verify(x => x.GetPaginatedFilesInfoAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Once);

            _encryptedFileRepositoryMock.Verify(x => x.GetTotalAsync(), Times.Once);
        }

        [Fact]
        public async Task
            When_Get_Paginated_Files_Info_By_Origin_Async_Is_Executed_With_Invalid_Current_Page_Then_Exception_Is_Thrown()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetPaginatedFilesMetadataByOriginAsync(0, 1, It.IsAny<string>()));

            Assert.Equal("currentPage", ex.ParamName);
        }

        [Fact]
        public async Task
            When_Get_Paginated_Files_Info_By_Origin_Async_Is_Executed_With_Invalid_Page_Size_Then_Exception_Is_Thrown()
        {
            var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
                _service.GetPaginatedFilesMetadataByOriginAsync(1, 0, It.IsAny<string>()));

            Assert.Equal("pageSize", ex.ParamName);
        }

        [Fact]
        public async Task
            When_Get_Paginated_Files_Info_By_Origin_Async_Is_Executed_Then_File_Repository_Is_Called()
        {
            _encryptedFileRepositoryMock.Setup(x =>
                    x.GetPaginatedFilesInfoByOriginAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(Task.FromResult(_fixture.CreateMany<EncryptedFile>()));

            _encryptedFileRepositoryMock.Setup(x => x.GetTotalByOriginAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(50));

            await _service.GetPaginatedFilesMetadataByOriginAsync(1, 10, "test");

            _encryptedFileRepositoryMock.Verify(
                x => x.GetPaginatedFilesInfoByOriginAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()),
                Times.Once);

            _encryptedFileRepositoryMock.Verify(x => x.GetTotalByOriginAsync(It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData(@"test,,,test...test\\\;;;;test", "testtesttesttest")]
        [InlineData(@"t@est$te%st*t((est^%#t{e}s|t", "testtesttesttest")]
        [InlineData(@"te", "te")]
        [InlineData(@"testtesttesttesttesttesttesttesttesttesttesttesttesttesttesttest",
            "testtesttesttesttesttesttesttesttesttesttesttesttesttesttesttest")]
        public void When_Prepare_Container_Name_Is_Executed_Then_Proper_Container_Name_Is_Returned(string origin,
            string expected)
        {
            var containerName = _service.PrepareContainerName(origin);

            Assert.Equal(expected, containerName);
        }

        [Theory]
        [InlineData(@"test,,,test...test\\\;;;;test", "a9554ea6-8bc3-451f-baa9-ba185591a2a0",
            "testtesttesttest_a9554ea6-8bc3-451f-baa9-ba185591a2a0")]
        [InlineData(@"t@est$te%st*t((est^%#t{e}s|t", "a9554ea6-8bc3-451f-baa9-ba185591a2a0",
            "testtesttesttest_a9554ea6-8bc3-451f-baa9-ba185591a2a0")]
        [InlineData(@"tEst,,,Test...teSt\\\;;;;tesT", "a9554ea6-8bc3-451f-baa9-ba185591a2a0",
            "tEstTestteSttesT_a9554ea6-8bc3-451f-baa9-ba185591a2a0")]
        public void When_Prepare_Blob_Name_Is_Executed_Then_Proper_Blob_Name_Is_Returned(string fileName, Guid fileId,
            string expected)
        {
            var blobName = _service.PrepareBlobName(fileName, fileId);

            Assert.Equal(expected, blobName);
        }

        [Fact]
        public void
            When_Validate_Prepared_Origin_And_File_Name_Is_Executed_For_Short_Origin_Data_Then_Exception_Is_Thrown()
        {
            var data = _fixture.Build<EncryptedFile>().With(x => x.Origin, "1").Create();

            var ex = Assert.Throws<ArgumentException>(() => { _service.ValidatePreparedOriginAndFileName(data); });

            Assert.Equal(nameof(data.Origin), ex.ParamName);
        }

        [Fact]
        public void
            When_Validate_Prepared_Origin_And_File_Name_Is_Executed_For_Long_Origin_Data_Then_Exception_Is_Thrown()
        {
            var data = _fixture.Build<EncryptedFile>().With(x => x.Origin, "1".PadLeft(64, '1')).Create();

            var ex = Assert.Throws<ArgumentException>(() => { _service.ValidatePreparedOriginAndFileName(data); });

            Assert.Equal(nameof(data.Origin), ex.ParamName);
        }

        [Fact]
        public void
            When_Validate_Prepared_Origin_And_File_Name_Is_Executed_For_Long_File_Name_Data_Then_Exception_Is_Thrown()
        {
            var data = _fixture.Build<EncryptedFile>().With(x => x.FileName, "1".PadLeft(1025, '1')).Create();

            var ex = Assert.Throws<ArgumentException>(() => { _service.ValidatePreparedOriginAndFileName(data); });

            Assert.Equal(nameof(data.FileName), ex.ParamName);
        }

        [Fact]
        public void When_Validate_Paging_Is_Executed_For_Invalid_Current_Page_Then_Exception_Is_Thrown()
        {
            var ex = Assert.Throws<ArgumentException>(() => _service.ValidatePaging(0, 1));

            Assert.Equal("currentPage", ex.ParamName);
        }

        [Fact]
        public void When_Validate_Paging_Is_Executed_For_Invalid_Page_Size_Then_Exception_Is_Thrown()
        {
            var ex = Assert.Throws<ArgumentException>(() => _service.ValidatePaging(1, 501));

            Assert.Equal("pageSize", ex.ParamName);
        }

        [Fact]
        public void When_Validate_Paging_Is_Executed_For_Invalid_Page_Size_And_Current_Page_Combination_Then_Exception_Is_Thrown()
        {
            Assert.Throws<ArgumentException>(() => _service.ValidatePaging(int.MaxValue, 1000));
        }
    }
}
