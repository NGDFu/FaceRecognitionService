目录结构：
    1. Common一些通用功能，比如接口返回值
    2. CrawleImages 一个http://www.4j4j.cn/mxtp/index_1.html 网站的超级简单的爬虫，可以在代码中看到保存图片路径
    3. Database 数据库相关，里面有一个LevelDB的实现，一个很简单的k-v数据库
    4. FaceIndex 内存的人脸特征索引库，包括入库和匹配
    5. FaceLabel 保存了人脸特征外的其他信息，比如在FaceIndex中的索引，如果是从文件取特征时的文件目录等
    6. FaceRecognition Http服务
    7. FaceRecognitionLib 取人脸特征库
FaceRecognition:
    1. 这个是整个项目的启动进程，是一个http服务，使用的是dotnet core 3.1里面的Asp.net Core
    2. 接口的编写在Controllers里面，这里面调用了Services里面的代码.
    3. Services是所有功能的整合实现，包括入库，匹配。
    4. 调试的时候可以从Controllers, 然后下断点进到Services.
    5. 进程启动后可以在浏览器里面输入http://localhost:60000/swagger，会出现自动生成的接口页面。
    6. 整个进程的配置信息在appsettings.json下面，比如端口配置，人脸匹配信息等
    7. 整个服务的日志会在进程中自动生成logs目录，一天一个日志文件。
入库流程：
    1. 取人脸特征
    2. 特征入到FaceIndex
    3. 非特征信息(具体看FaceLabel下的FaceInfo.cs)保存到FaceLabel下面。
匹配流程：
    1. 取人脸特征
    2. 从FaceIndex中取配置文件里面的TopK个人脸特征，同时会取到对应特征的索引
    3. 将该索引从FaceLabel中取人脸对应的非特征信息