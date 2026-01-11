/*
This file in the main entry point for defining grunt tasks and using grunt plugins.
Click here to learn more. https://go.microsoft.com/fwlink/?LinkID=513275&clcid=0x409
*/
module.exports = function (grunt) {
  grunt.initConfig({
    copy: {
      main: {
        files: [
          {
            expand: true, // Required to use cwd
            cwd: 'path/to/source/files', // Set the source working directory
            src: ['**/*'], // Copy all files and subfolders
            dest: 'dist/destination/folder/' // The destination folder
          }
        ]
      },
      // You can add more targets here for different copy tasks
      html_files_only: {
        files: [
          {
            expand: true,
            cwd: 'TempHtmlFiles/',
            src: ['**/*.html'], // Only copy HTML files
            dest: 'wwwroot/Htmlfiles/'
          }
        ]
      }
    }
  });

  // Load the plugin that provides the "copy" task.
  grunt.loadNpmTasks('grunt-contrib-copy');

  // Define default task that runs the copy operation(s)
  grunt.registerTask('default', ['copy:main']);
};