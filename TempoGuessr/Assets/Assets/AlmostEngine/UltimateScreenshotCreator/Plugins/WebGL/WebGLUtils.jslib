
var ImageDownloaderPlugin = {

	_ExportImage: function (data, filename, format) {

		var imageData = Pointer_stringify(data);
		var imageFileName = Pointer_stringify(filename);
		var contentType = 'image/' + format;

		// Converts the image data to binary
		// From http://stackoverflow.com/questions/14967647/
		// encode-decode-image-with-base64-breaks-image (2013-04-21)
		function fixBinary(data) {
			var length = data.length;
			var binary = new ArrayBuffer(length);
			var arr = new Uint8Array(binary);
			for (var i = 0; i < length; i++) {
				arr[i] = data.charCodeAt(i);
			}
			return binary;
		}
		var binary = fixBinary(atob(imageData));

		// Creates an image Blob
		var imageBlob = new Blob([binary], { type: contentType });

		if (window.navigator.msSaveOrOpenBlob != null) {
			// For old browsers
			window.navigator.msSaveBlob(imageBlob, imageFileName);
		}
		else {
			// Creates a clickable link that will download the image
			var link = document.createElement('a');
			link.download = imageFileName;
			link.innerHTML = 'DownloadFile';
			link.setAttribute('download', imageFileName);
			link.style.display = 'none';

			// Creates the click URL
			if (window.webkitURL != null) {
				link.href = window.webkitURL.createObjectURL(imageBlob);
			}
			else {
				link.href = window.URL.createObjectURL(imageBlob);
				document.body.appendChild(link);
			}

			//Calling the link click action
			link.click();

			// Clean
			if (window.webkitURL == null) {
				document.body.removeChild(link);
			}
		}
	}

};

mergeInto(LibraryManager.library, ImageDownloaderPlugin);