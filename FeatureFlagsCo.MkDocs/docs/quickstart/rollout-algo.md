## 算法 & 优势
当前算法以CryptoMD5 Hash的方式分配用户至不同的百分比。目前采用此算法的云印主要考虑到了如下因素:

- 用户调用flags是循序渐进的，往往系统并不知道会使用此flags的总人数。
- 因为Feature Flags功能的特殊性，用户对Feature Flag的整体请求速度要尽量控制在50ms以内 (其中包括了网络传递速度、Flags返回值的判断过程)。所以在做rollout percentage判断时，应尽量减少判断程序与数据库(包括缓存)之间的读写次数和复杂度。
- Feature Flags的数量会很多，一个应用中的一个用户可能会对10个Flags进行请求。对于很多互联网与物联网产品，调用频次和数量都会很高
- 应尽量按照百分比均匀分配。Crypto MD5方式计算的Hash给我们了实现均匀分配的简单方案


### 算法代码(C#):

    public bool IfBelongRolloutPercentage(string userFFKeyId, double[] rolloutPercentageRange)
    {
        byte[] hashedKey = new MD5CryptoServiceProvider().ComputeHash(ASCIIEncoding.ASCII.GetBytes(userFFKeyId));
        int a0 = BitConverter.ToInt32(hashedKey, 0);
        double y = Math.Abs((double)a0 / (double)int.MinValue);
        if (y >= rolloutPercentageRange[0] && y <= rolloutPercentageRange[1])
        {
            return true;
        }

        return false;
    }


### 测试代码与结果

对于上面的代码，我们做了5轮测试，每轮分别做100、1000、10000个基于用户GUID的百分比分配测试：


**结果如下**: 

##Rround 0##  100 sample: 32; 1000 sample: 326; 10000 sample: 3315;   
##Rround 1##  100 sample: 32; 1000 sample: 337; 10000 sample: 3335;   
##Rround 2##  100 sample: 34; 1000 sample: 322; 10000 sample: 3319;   
##Rround 3##  100 sample: 41; 1000 sample: 332; 10000 sample: 3276;   
##Rround 4##  100 sample: 36; 1000 sample: 329; 10000 sample: 3364;   
##Rround 5##  100 sample: 36; 1000 sample: 322; 10000 sample: 3315;   
##Rround 6##  100 sample: 36; 1000 sample: 351; 10000 sample: 3319;   
##Rround 7##  100 sample: 32; 1000 sample: 348; 10000 sample: 3415;   
##Rround 8##  100 sample: 30; 1000 sample: 316; 10000 sample: 3271;   
##Rround 9##  100 sample: 31; 1000 sample: 340; 10000 sample: 3348;

**测试代码(C#)**:
    public string IfBelongRolloutPercentage()
    {
        string returnValue = "";
        for(int k = 0; k < 10; k++)
        {
            returnValue += $"   ##Rround {k}## ";
            int trueCount = 0;
            for (int i = 0; i < 100; i++)
            {
                if (_variationService.IfBelongRolloutPercentage(Guid.NewGuid().ToString(), new double[] { 0.0, 0.333 }))
                    trueCount++;
            }
            returnValue += $" 100 sample: {trueCount};";
            trueCount = 0;
            for (int i = 0; i < 1000; i++)
            {
                if (_variationService.IfBelongRolloutPercentage(Guid.NewGuid().ToString(), new double[] { 0.0, 0.333 }))
                    trueCount++;
            }
            returnValue += $" 1000 sample: {trueCount};";
            trueCount = 0;
            for (int i = 0; i < 10000; i++)
            {
                if (_variationService.IfBelongRolloutPercentage(Guid.NewGuid().ToString(), new double[] { 0.0, 0.333 }))
                    trueCount++;
            }
            returnValue += $" 10000 sample: {trueCount};";
        }
        return returnValue;
    }

### 补充
我们目前还有一些其他的方案来实现百分比均匀分配方案。比如：

> You can employ standard scheme for such tasks: (a0 + Q*a1 + Q^2*a2 + Q^3*a3 + ...) % M where M is a very large prime number and Q is coefficient of your choice. (https://stackoverflow.com/questions/3816466/evenly-distributed-hash-function)

    double hash(int... a) {
        int Q = 433494437;
        int result = 0;
        for (int n : a) {
            result = result * Q + n * n;
        }
        result *= Q;
        return (double) result / Integer.MIN_VALUE;
    }

我们还尝试过用神经网络去做均匀分布的训练，目前测试效果不如使用CryptoMD5 Hash的方式，有待继续提高

所有的平均分配方案，我们会在未来继续努力。



## 弊端
此算法并非严格意义的使用计数方式进行百分比分配，在量少的情况下，很可能造成与百分比分配不一致的情况。

### 解决方案

对于少量用户的情况，可以通过设置用户组的方式由使用人员手工分配。用户组即将指定用户分配到一个组里。

如当前一共200个用户，我们希望按照30%,40%,30%的比率分配给不同的用户不同的返回值