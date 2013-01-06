CXX = g++
DEBUG = -g
CXXFLAGS = -m32 $(DEBUG) -Wall -pedantic
CNAME = v8_helper
FSNAME1 = LEmbedding
FSNAME2 = NEmbedding
JSENGINE = JSEngineUtils
FSTEST = test
V8PATH = ~/Coding/new_v8/v8/out/ia32.release/libv8_{base,snapshot}.a
OBJS = $(CNAME).dylib $(FSNAME).fs


all: $(FSNAME).exe


$(CNAME).dylib: $(CNAME).o
	$(CXX) $(CXXFLAGS) -dynamiclib -Iinclude -undefined suppress -flat_namespace $(CNAME).o -o $(CNAME).dylib $(V8PATH) -lpthread

.c.o :
	$(CC) $(CFLAGS) -c $(.SOURCE)

$(FSNAME).exe: $(CNAME).dylib
	fsharpc $(JSENGINE).fs  $(FSNAME1).fs $(FSNAME2).fs $(FSTEST).fs

check-syntax:
	$(CXX) -o nul -S ${CHK_SOURCES}

clean:
	rm $(CNAME).dylib $(CNAME).o $(FSNAME).exe
